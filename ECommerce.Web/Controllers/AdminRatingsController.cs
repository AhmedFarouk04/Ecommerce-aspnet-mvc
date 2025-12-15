using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Core.Interfaces;
using ECommerce.Web.ViewModels;
using ECommerce.Infrastructure.Identity;

[Authorize(Roles = "Admin")]
public class AdminRatingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminRatingsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(
        int? stars,
        int? productId,
        string? username,
        string? email,
        string sort = "newest",
        int page = 1,
        int pageSize = 15)
    {
        var ratings = _unitOfWork.Ratings.Query().ToList();

       
        if (stars.HasValue)
            ratings = ratings.Where(r => r.Stars == stars.Value).ToList();

        if (productId.HasValue)
            ratings = ratings.Where(r => r.ProductId == productId.Value).ToList();

        if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(email))
        {
            var matchedUsers = _userManager.Users
                .Where(u =>
                       (!string.IsNullOrEmpty(username) && u.UserName.Contains(username)) ||
                       (!string.IsNullOrEmpty(email) && u.Email.Contains(email)))
                .Select(u => u.Id)
                .ToList();

            ratings = ratings.Where(r => matchedUsers.Contains(r.UserId)).ToList();
        }

       
        ratings = sort switch
        {
            "oldest" => ratings.OrderBy(r => r.CreatedAt).ToList(),
            "stars_high" => ratings.OrderByDescending(r => r.Stars).ToList(),
            "stars_low" => ratings.OrderBy(r => r.Stars).ToList(),
            _ => ratings.OrderByDescending(r => r.CreatedAt).ToList(),
        };

        
        int totalItems = ratings.Count;
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var pageList = ratings
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var userIds = pageList.Select(r => r.UserId).Distinct().ToList();
        var productIds = pageList.Select(r => r.ProductId).Distinct().ToList();

        var usersDict = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        var productsDict = _unitOfWork.Products
            .GetAll()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionary(p => p.Id);

       
        var vm = pageList.Select(r =>
        {
            usersDict.TryGetValue(r.UserId, out var user);
            productsDict.TryGetValue(r.ProductId, out var product);

            return new RatingAdminViewModel
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = product?.Name ?? "Unknown Product",

                Stars = r.Stars,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,

                UserId = r.UserId,
                UserName = user?.UserName ?? "Unknown",
                Email = user?.Email,
                UserImage = user?.ImageUrl,
                UserThumbnail = user?.ThumbnailUrl
            };
        }).ToList();

        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.PageSize = pageSize;

        return View(vm);
    }

    
    public async Task<IActionResult> Details(int id)
    {
        var rating = _unitOfWork.Ratings.GetById(id);
        if (rating == null) return NotFound();

        var user = await _userManager.FindByIdAsync(rating.UserId);
        var product = _unitOfWork.Products.GetById(rating.ProductId);

        return View(new RatingAdminViewModel
        {
            Id = rating.Id,
            ProductId = rating.ProductId,
            ProductName = product?.Name,
            Stars = rating.Stars,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt,

            UserId = rating.UserId,
            UserName = user?.UserName,
            Email = user?.Email,
            UserThumbnail = user?.ThumbnailUrl
        });
    }

    
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var rating = _unitOfWork.Ratings.GetById(id);
        if (rating == null) return NotFound();

        _unitOfWork.Ratings.Delete(rating);
        _unitOfWork.Complete();

        TempData["Success"] = "Rating deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
