using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class WishlistController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public WishlistController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);


    [HttpPost]
    public IActionResult Add(int productId)
    {
        if (CurrentUserId == null)
            return Json(new { success = false, message = "You must be logged in." });

        var product = _unitOfWork.Products.GetById(productId);
        if (product == null)
            return Json(new { success = false, message = "Product not found." });

        if (!_unitOfWork.Wishlists.Exists(CurrentUserId, productId))
        {
            _unitOfWork.Wishlists.Add(new Wishlist
            {
                UserId = CurrentUserId,
                ProductId = productId
            });

            _unitOfWork.Complete();
        }

        int count = _unitOfWork.Wishlists
            .GetUserWishlist(CurrentUserId)
            .Count();

        return Json(new
        {
            success = true,
            isInWishlist = true,
            count
        });
    }

    [HttpPost]
    public IActionResult Remove(int productId)
    {
        if (CurrentUserId == null)
            return Json(new { success = false, message = "You must be logged in." });

        var item = _unitOfWork.Wishlists.GetItem(CurrentUserId, productId);
        if (item == null)
            return Json(new { success = false, message = "Item not found in wishlist." });

        _unitOfWork.Wishlists.Delete(item);
        _unitOfWork.Complete();

        int count = _unitOfWork.Wishlists
            .GetUserWishlist(CurrentUserId)
            .Count();

        return Json(new
        {
            success = true,
            isInWishlist = false,
            count
        });
    }


    public IActionResult Index()
    {
        if (CurrentUserId == null)
            return RedirectToAction("Index", "Products");

        var items = _unitOfWork.Wishlists.GetUserWishlist(CurrentUserId);
        return View(items);
    }

    [HttpGet]
    public IActionResult Count()
    {
        if (CurrentUserId == null)
            return Json(new { count = 0 });

        int count = _unitOfWork.Wishlists.GetUserWishlist(CurrentUserId).Count();
        return Json(new { count });
    }
}
