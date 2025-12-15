using ECommerce.Application.Services;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.Services;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminReviewsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminActivityLogger _logger;

    public AdminReviewsController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        AdminActivityLogger logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    
    public async Task<IActionResult> Index(
        string? search,
        int? productId,
        string? userId,
        string sort = "newest",
        int page = 1,
        int pageSize = 12)
    {
        var query = _unitOfWork.Reviews
            .Query()
            .Include(r => r.Product)
            .AsQueryable();

        
        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.ToLower();
            query = query.Where(r =>
                (r.Comment != null && r.Comment.ToLower().Contains(s)) ||
                (r.Product != null && r.Product.Name.ToLower().Contains(s)) ||
                (r.UserName != null && r.UserName.ToLower().Contains(s))
            );
        }

       
        if (productId.HasValue)
            query = query.Where(r => r.ProductId == productId.Value);

        
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(r => r.UserId == userId);

       
        query = sort switch
        {
            "oldest" => query.OrderBy(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt),
        };

       
        int totalItems = await query.CountAsync();
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));

        page = Math.Clamp(page, 1, totalPages);

        var pageData = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

       
        var userIds = pageData.Select(r => r.UserId).Distinct().ToList();

        var usersDict = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u);

       
        var vmList = pageData.Select(r =>
        {
            usersDict.TryGetValue(r.UserId, out var user);

            return new AdminReviewViewModel
            {
                Id = r.Id,

                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "Unknown",
                ProductThumbnail = r.Product?.ThumbnailUrl,

                UserId = r.UserId,
                UserName = r.UserName ?? user?.UserName ?? "Unknown",
                UserThumbnail = user?.ThumbnailUrl ?? "default.png",

                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            };
        }).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Search = search;
        ViewBag.Sort = sort;

        return View(vmList);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = _unitOfWork.Reviews.GetById(id);
        if (review == null)
            return NotFound();

        _unitOfWork.Reviews.Delete(review);
        _unitOfWork.Complete();

        await _logger.LogAsync(
            _userManager.GetUserId(User),
            "Delete Review",
            $"Deleted Review #{id}"
        );

        TempData["Success"] = "Review deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
