using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            ICartService cartService,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        
        public IActionResult Index()
        {
            CartDto cart = _cartService.GetCart(UserId);

            if (cart.IsEmpty)
                return RedirectToAction("Index", "Cart");

            var vm = new CheckoutViewModel
            {
                Cart = cart
            };

            return View(vm);
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart(UserId);

            if (cart.IsEmpty)
                return RedirectToAction("Index", "Cart");

            ModelState.Remove(nameof(model.Cart));

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                return View("Index", model);
            }

            _unitOfWork.BeginTransaction();

            try
            {
                var orderItems = cart.Items.Select(item =>
                {
                    var product = _unitOfWork.Products.GetById(item.ProductId);

                    if (product == null || product.Stock < item.Quantity)
                        throw new Exception("Stock issue");

                    return new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };
                }).ToList();

                var order = new Order
                {
                    UserId = UserId,
                    FullName = model.FullName,
                    Address = model.Address,
                    TotalAmount = cart.Total,
                    PaymentStatus = PaymentStatus.Unpaid,
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    Items = orderItems
                };

                _unitOfWork.Orders.Add(order);
                _unitOfWork.Complete();
                _unitOfWork.Commit();

                return RedirectToAction("Start", "Payment", new { orderId = order.Id });
            }
            catch
            {
                _unitOfWork.Rollback();
                TempData["CartError"] = "Could not place order.";
                return RedirectToAction("Index", "Cart");
            }
        }


    }
}
