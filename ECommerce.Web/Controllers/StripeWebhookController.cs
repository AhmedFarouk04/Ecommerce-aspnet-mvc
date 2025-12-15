using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using System.IO;

namespace ECommerce.Web.Controllers
{
    [ApiController]
    [Route("stripe-webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            EmailService emailService,
            UserManager<ApplicationUser> userManager,
            ILogger<StripeWebhookController> logger)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _config["Stripe:WebhookSecret"];

            Stripe.Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid Stripe signature.");
                return BadRequest("Invalid Stripe signature.");
            }

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    var sessionCompleted = stripeEvent.Data.Object as Session;
                    if (sessionCompleted != null)
                        await HandleCheckoutCompletedAsync(sessionCompleted);
                    break;

                case "checkout.session.expired":
                    var sessionExpired = stripeEvent.Data.Object as Session;
                    if (sessionExpired != null)
                        HandleCheckoutExpired(sessionExpired);
                    break;
            }

            return Ok();
        }

        private async Task HandleCheckoutCompletedAsync(Session session)
        {
            if (!int.TryParse(session.ClientReferenceId, out var orderId))
                return;

            var order = _unitOfWork.Orders.GetOrderWithItems(orderId);
            if (order == null || order.PaymentStatus == PaymentStatus.Paid)
                return;

            foreach (var item in order.Items)
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);
                if (product == null)
                    continue;

                product.Stock -= item.Quantity;
                _unitOfWork.Products.Update(product);
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = OrderStatus.Processing;
            order.PaymentReference = session.PaymentIntentId ?? session.Id;

            _unitOfWork.Orders.Update(order);
            _unitOfWork.Complete();
        }


        private void HandleCheckoutExpired(Session session)
        {
            if (session.ClientReferenceId == null ||
                !int.TryParse(session.ClientReferenceId, out var orderId))
                return;

            var order = _unitOfWork.Orders.GetById(orderId);
            if (order != null && order.PaymentStatus == PaymentStatus.Unpaid)
            {
                order.Status = OrderStatus.Pending;
                _unitOfWork.Orders.Update(order);
                _unitOfWork.Complete();
            }
        }

        private string BuildUserPaidEmail(Core.Entities.Order order) =>
            $@"
            <div style='font-family:Arial;padding:20px'>
                <h2>Thank you for your payment!</h2>
                <p>Your order <b>#{order.Id}</b> has been paid successfully.</p>
                <p>Total Amount: <b>{order.TotalAmount:C}</b></p>
                <p>Status: <b>{order.Status}</b></p>
            </div>";

        private string BuildAdminPaidEmail(Core.Entities.Order order) =>
            $@"
            <div style='font-family:Arial;padding:20px'>
                <h2>Order Payment Confirmed</h2>
                <p>Order ID: <b>#{order.Id}</b></p>
                <p>Total Amount: <b>{order.TotalAmount:C}</b></p>
                <p>Payment Status: {order.PaymentStatus}</p>
                <p>Status: {order.Status}</p>
            </div>";
    }
}
