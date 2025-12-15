using System;
using System.Linq;
using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

   
    [AllowAnonymous]
    [HttpGet]
    public IActionResult List(int productId)
    {
        var reviews = _unitOfWork.Reviews.GetProductReviews(productId);

        string userId = null;
        if (User.Identity?.IsAuthenticated == true)
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = reviews
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserName = r.UserName,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                CreatedAtFormatted = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                IsOwner = userId != null && r.UserId == userId
            })
            .ToList();

        return Json(result);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(int productId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return Json(new { success = false, message = "Comment is required." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.Identity?.Name ?? "User";

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            UserName = userName,
            Comment = comment.Trim()
        };

        _unitOfWork.Reviews.Add(review);
        _unitOfWork.Complete();

        return Json(new
        {
            success = true,
            id = review.Id,
            userName = review.UserName,
            comment = review.Comment,
            createdAt = review.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        });
    }

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return Json(new { success = false, message = "Comment cannot be empty." });

        var review = _unitOfWork.Reviews.GetById(id);
        if (review == null)
            return Json(new { success = false, message = "Review not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (review.UserId != userId && !isAdmin)
            return Json(new { success = false, message = "Not allowed to edit this review." });

        review.Comment = comment.Trim();
        review.CreatedAt = DateTime.Now;

        _unitOfWork.Reviews.Update(review);
        _unitOfWork.Complete();

        return Json(new
        {
            success = true,
            updatedComment = review.Comment,
            updatedTime = review.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        });
    }

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var review = _unitOfWork.Reviews.GetById(id);
        if (review == null)
            return Json(new { success = false, message = "Review not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (userId == null || (review.UserId != userId && !isAdmin))
            return Json(new { success = false, message = "Not allowed to delete this review." });

        _unitOfWork.Reviews.Delete(review);
        _unitOfWork.Complete();

        return Json(new { success = true });
    }
}
