using ECommerce.Application.Interfaces;
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace ECommerce.Web.Controllers
{
    [Authorize]
    [Route("Payment")]
    public class PaymentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly ICartService _cartService;

        public PaymentController(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _cartService = cartService;
        }

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

      
        [HttpGet("Start")]
        public IActionResult Start(int orderId)
        {
            if (orderId <= 0)
                return BadRequest("Invalid order id");

            var order = _unitOfWork.Orders.GetOrderWithItems(orderId);

            if (order == null || !order.Items.Any())
                return RedirectToAction("MyOrders", "Orders");

           
            if (order.PaymentStatus != PaymentStatus.Unpaid)
                return RedirectToAction("MyOrders", "Orders");

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                ClientReferenceId = order.Id.ToString(),
                SuccessUrl = $"{domain}/Payment/Success?orderId={order.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Payment/Cancel?orderId={order.Id}",
                LineItems = order.Items.Select(item => new SessionLineItemOptions
                {
                    Quantity = item.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.UnitPrice * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product?.Name ?? $"Product #{item.ProductId}"
                        }
                    }
                }).ToList()
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

       
        [HttpGet("Success")]
        public IActionResult Success(int orderId, string session_id)
        {
            var order = _unitOfWork.Orders.GetOrderWithItems(orderId);

            if (order == null)
                return RedirectToAction("Index", "Home");

            if (order.PaymentStatus == PaymentStatus.Paid)
                return View();

            _unitOfWork.BeginTransaction();

            try
            {
                foreach (var item in order.Items)
                {
                    var product = _unitOfWork.Products.GetById(item.ProductId);

                    if (product == null || product.Stock < item.Quantity)
                    {
                        order.Status = OrderStatus.Cancelled;
                        _unitOfWork.Complete();
                        _unitOfWork.Commit();
                        return RedirectToAction("Failed");
                    }

                    product.Stock -= item.Quantity;
                }

                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Completed;
                order.PaymentReference = session_id;

                _unitOfWork.Complete();
                _unitOfWork.Commit();

                _cartService.Clear(UserId);

                ViewBag.OrderId = orderId;
                return View();
            }
            catch
            {
                _unitOfWork.Rollback();
                return RedirectToAction("Failed");
            }
        }

      
        [HttpGet("Cancel")]
        public IActionResult Cancel(int orderId)
        {
            var order = _unitOfWork.Orders.GetById(orderId);

            if (order != null && order.PaymentStatus == PaymentStatus.Unpaid)
            {
                order.Status = OrderStatus.Cancelled;
                _unitOfWork.Complete();
            }

         
            return RedirectToAction("Index", "Cart");
        }

      
        [HttpGet("Failed")]
        public IActionResult Failed()
        {
            return View();
        }
    }
}
