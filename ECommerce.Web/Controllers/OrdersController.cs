using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult MyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = _unitOfWork.Orders.GetOrdersForUser(userId);
        return View(orders);
    }

    public IActionResult Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = _unitOfWork.Orders.GetOrderWithItems(id);

        if (order == null || order.UserId != userId)
            return NotFound();

        return View(order);
    }

    public IActionResult EditAddress(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = _unitOfWork.Orders.GetById(id); 

        if (order == null || order.UserId != userId)
            return NotFound();

        if (order.Status != OrderStatus.Pending || order.PaymentStatus == PaymentStatus.Paid)
        {
            TempData["Error"] = "Cannot edit address for this order.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new EditAddressViewModel
        {
            OrderId = order.Id,
            FullName = order.FullName,
            Address = order.Address
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAddress(EditAddressViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var order = _unitOfWork.Orders.GetTracked(model.OrderId);

        if (order == null || order.UserId != userId)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction(nameof(MyOrders));
        }

        if (order.Status != OrderStatus.Pending || order.PaymentStatus == PaymentStatus.Paid)
        {
            TempData["Error"] = "Cannot edit address for this order.";
            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }

        order.FullName = model.FullName;
        order.Address = model.Address;

        _unitOfWork.Complete();  

        TempData["Success"] = "Shipping address updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.OrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = _unitOfWork.Orders.GetOrderWithItems(id);

        if (order == null || order.UserId != userId)
            return NotFound();

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled || order.PaymentStatus == PaymentStatus.Paid)
        {
            TempData["Error"] = "Cannot cancel this order.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (order.Items != null)
        {
            foreach (var item in order.Items)
            {
                if (item.Product != null)  
                {
                    item.Product.Stock += item.Quantity;
                    
                }
            }
        }

        order.Status = OrderStatus.Cancelled;
        _unitOfWork.Complete();  

        TempData["Success"] = $"Order #{order.Id} has been cancelled successfully and stock has been restored.";
        return RedirectToAction(nameof(MyOrders));
    }
}