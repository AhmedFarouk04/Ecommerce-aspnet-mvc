using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

[Authorize]
public class RatingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public RatingsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public IActionResult Rate(int productId, int stars, string? comment)
    {
        
        if (stars < 1 || stars > 5)
        {
            return Json(new { success = false, message = "Invalid rating value." });
        }

        
        var product = _unitOfWork.Products.GetById(productId);
        if (product == null)
        {
            return Json(new { success = false, message = "Product not found." });
        }

       
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "User not authenticated." });
        }

        
        var existing = _unitOfWork.Ratings
            .GetProductRatings(productId)
            .FirstOrDefault(r => r.UserId == userId);

        if (existing != null)
        {
            existing.Stars = stars;
            existing.Comment = string.IsNullOrWhiteSpace(comment) ? null : comment;

            _unitOfWork.Ratings.Update(existing);
        }
        else
        {
            var rating = new Rating
            {
                UserId = userId,
                ProductId = productId,
                Stars = stars,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            _unitOfWork.Ratings.Add(rating);
        }

        _unitOfWork.Complete();

       
        var ratings = _unitOfWork.Ratings.GetProductRatings(productId).ToList();
        var average = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

        return Json(new
        {
            success = true,
            message = "Rating submitted successfully.",
            averageRating = average,
            totalRatings = ratings.Count,
            userStars = stars
        });
    }
}
