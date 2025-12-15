using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Interfaces;
using ECommerce.Web.Services;
using ECommerce.Web.MappingProfiles;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class CartController : Controller
{
    private readonly SessionCartService _sessionCart;
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;

    public CartController(
        SessionCartService sessionCart,
        ICartService cartService,
        IUnitOfWork unitOfWork)
    {
        _sessionCart = sessionCart;
        _cartService = cartService;
        _unitOfWork = unitOfWork;
    }

    private bool IsAuthenticated =>
        User.Identity?.IsAuthenticated == true;

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    
    private int GetAvailableStock(int productId)
    {
        var product = _unitOfWork.Products.GetById(productId);
        if (product == null) return 0;

        int cartQty = IsAuthenticated
            ? _unitOfWork.Carts.GetByUserId(UserId)?
                .Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0
            : _sessionCart.GetCart()
                .FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;

        return product.Stock - cartQty;
    }

   
    private CartDto GetCurrentCart()
    {
        if (IsAuthenticated)
            return _cartService.GetCart(UserId);

        return _sessionCart.GetCart().ToCartDto();
    }

  
    public IActionResult Index()
    {
        var cart = GetCurrentCart();
        return View(cart);
    }

   
    [HttpPost]
    public IActionResult AddAjax(int id)
    {
        var product = _unitOfWork.Products.GetById(id);
        if (product == null)
            return Json(new { success = false, message = "Product not found." });

        int availableBefore = GetAvailableStock(id);
        if (availableBefore < 1)
            return Json(new { success = false, message = "Out of stock." });

        if (IsAuthenticated)
        {
            _cartService.Add(UserId, id);
        }
        else
        {
            var cart = _sessionCart.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item == null)
            {
                cart.Add(new ECommerce.Web.ViewModels.CartItem
                {
                    ProductId = id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
            }
            else
            {
                item.Quantity++;
            }

            _sessionCart.SaveCart(cart);
        }

        var cartDto = GetCurrentCart();

        return Json(new
        {
            success = true,
            message = "Added to cart!",
            productId = id,
            availableStock = GetAvailableStock(id),
            count = cartDto.Count,
            total = cartDto.Total
        });
    }

    
    [HttpPost]
    public IActionResult RemoveAjax(int id)
    {
        if (IsAuthenticated)
        {
            _cartService.Remove(UserId, id);
        }
        else
        {
            var cart = _sessionCart.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                _sessionCart.SaveCart(cart);
            }
        }

        var cartDto = GetCurrentCart();

        return Json(new
        {
            success = true,
            productId = id,
            availableStock = GetAvailableStock(id),
            count = cartDto.Count,
            total = cartDto.Total
        });
    }

  
    [HttpPost]
    public IActionResult UpdateQty(int productId, int delta)
    {
        if (!IsAuthenticated)
            return Json(new { success = false, message = "Unauthorized" });

        var result = _cartService.UpdateQuantity(UserId, productId, delta);

        if (!result.Success)
            return Json(result);

        return Json(new
        {
            success = true,
            productId,
            quantity = result.Quantity,
            itemTotal = result.ItemTotal,
            availableStock = GetAvailableStock(productId),
            count = result.Count,
            total = result.Total
        });
    }

    [HttpGet]
    public IActionResult State()
    {
        var cart = GetCurrentCart();

        return Json(new
        {
            count = cart.Count,
            total = cart.Total,
            items = cart.Items.Select(x => new
            {
                productId = x.ProductId,
                name = x.ProductName,
                price = x.Price,
                quantity = x.Quantity,
                total = x.Total,
                image = x.ImageUrl
            })
        });
    }
}
