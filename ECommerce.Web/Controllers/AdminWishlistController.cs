using ECommerce.Core.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Identity;

[Authorize(Roles = "Admin")]
public class AdminWishlistController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminWishlistController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    
    public async Task<IActionResult> Index(
        string? search,
        int? productId,
        string? userId,
        int page = 1,
        int pageSize = 15)
    {
        var query = _unitOfWork.Wishlists
            .Query()
            .Include(w => w.Product)
            .AsQueryable();

      
        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.ToLower();
            query = query.Where(w =>
                (w.Product != null && w.Product.Name.ToLower().Contains(s)) ||
                (w.UserId != null && w.UserId.ToLower().Contains(s))
            );
        }

       
        if (productId.HasValue)
            query = query.Where(w => w.ProductId == productId.Value);

        
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(w => w.UserId == userId);

        
        int totalItems = await query.CountAsync();
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));

        page = Math.Clamp(page, 1, totalPages);

        var pageData = await query
            .OrderByDescending(w => w.AddedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

       
        var userIds = pageData.Select(w => w.UserId).Distinct().ToList();

        var usersDict = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u);

      
        var result = pageData.Select(w =>
        {
            usersDict.TryGetValue(w.UserId, out var usr);

            return new WishlistAdminViewModel
            {
                Id = w.Id,

                UserId = w.UserId,
                UserName = usr?.UserName ?? "Unknown",
                UserThumbnail = usr?.ThumbnailUrl ?? "default.png",

                ProductId = w.ProductId,
                ProductName = w.Product?.Name ?? "Unknown",
                ProductThumbnail = w.Product?.ThumbnailUrl,

                AddedAt = w.AddedAt
            };
        }).ToList();

        
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.ProductId = productId;
        ViewBag.UserId = userId;
        ViewBag.Search = search;

        return View(result);
    }

   
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var item = _unitOfWork.Wishlists.GetById(id);
        if (item == null)
            return NotFound();

        _unitOfWork.Wishlists.Delete(item);
        _unitOfWork.Complete();

        TempData["Success"] = "Wishlist item deleted!";
        return RedirectToAction(nameof(Index));
    }
}
