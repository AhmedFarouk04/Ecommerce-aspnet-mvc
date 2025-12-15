using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminOrdersController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    
    public IActionResult Index(
        string search,
        string status,
        string paymentStatus,
        string sort,
        int page = 1,
        int pageSize = 10)
    {
        var orders = _unitOfWork.Orders
            .GetAll()
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var usersDict = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        var list = orders.Select(o =>
        {
            usersDict.TryGetValue(o.UserId, out var user);

            return new AdminOrderViewModel
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = user?.UserName,
                Email = user?.Email,
                UserImage = user?.ImageUrl,
                UserThumbnail = user?.ThumbnailUrl,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                ItemsCount = o.Items?.Count ?? 0
            };
        }).ToList();


        if(!string.IsNullOrEmpty(status) &&
    Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
{
            list = list
                .Where(o => o.Status == orderStatus)
                .ToList();
        }

        if (!string.IsNullOrEmpty(paymentStatus) &&
            Enum.TryParse<PaymentStatus>(paymentStatus, true, out var payStatus))
        {
            list = list
                .Where(o => o.PaymentStatus == payStatus)
                .ToList();
        }


        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            bool parsedId = int.TryParse(term, out var orderId);

            list = list.Where(o =>
                   (parsedId && o.Id == orderId)
                || (!string.IsNullOrEmpty(o.UserName) &&
                    o.UserName.Contains(term, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(o.Email) &&
                    o.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

       
        list = sort switch
        {
            "date_asc" => list.OrderBy(o => o.OrderDate).ToList(),
            "total_high" => list.OrderByDescending(o => o.TotalAmount).ToList(),
            "total_low" => list.OrderBy(o => o.TotalAmount).ToList(),
            "user_asc" => list.OrderBy(o => o.UserName).ToList(),
            "user_desc" => list.OrderByDescending(o => o.UserName).ToList(),
            "status" => list.OrderBy(o => o.Status).ToList(),
            "payment" => list.OrderBy(o => o.PaymentStatus).ToList(),
            _ => list.OrderByDescending(o => o.OrderDate).ToList() 
        };

        
        int totalItems = list.Count;
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var pageData = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.PageSize = pageSize;

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.PaymentStatus = paymentStatus;
        ViewBag.Sort = sort;

        return View(pageData);
    }


    public IActionResult Details(int id, string? returnUrl = null)
    {
        var order = _unitOfWork.Orders.GetOrderWithItems(id);
        if (order == null) return NotFound();

        ViewBag.ReturnUrl = returnUrl;
        return View(order);
    }



    public IActionResult ChangeStatus(int id, string? returnUrl = null)
    {
        var order = _unitOfWork.Orders.GetById(id);
        if (order == null) return NotFound();

        ViewBag.ReturnUrl = returnUrl;
        return View(order);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeStatus(int id, string status, string? returnUrl = null)
    {
        var order = _unitOfWork.Orders.GetById(id);
        if (order == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrderStatus>(status, true, out var newStatus))
        {
            order.Status = newStatus;

            _unitOfWork.Orders.Update(order);
            _unitOfWork.Complete();

            TempData["Success"] = $"Order #{order.Id} status updated to {order.Status}.";
        }
        else
        {
            TempData["Error"] = "Invalid order status.";
        }

        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

}
