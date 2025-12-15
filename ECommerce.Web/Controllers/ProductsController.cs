using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.Services;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Security.Claims;

namespace ECommerce.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AdminActivityLogger _logger;
        private readonly ImageProcessingService _imageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        private readonly IMemoryCache _cache;
        private readonly ICartService _cartService;
        private readonly SessionCartService _sessionCart;

        public ProductsController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            AdminActivityLogger logger,
            ImageProcessingService imageService,
            UserManager<ApplicationUser> userManager,
            IProductService productService,
            IMemoryCache cache,
            ICartService cartService,
            SessionCartService sessionCart)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _imageService = imageService;
            _userManager = userManager;
            _productService = productService;
            _cache = cache;
            _cartService = cartService;
            _sessionCart = sessionCart;
        }

        [AllowAnonymous]
        public IActionResult Index(
       string? search,
       int? categoryId,
       string? sort,
       decimal? minPrice,
       decimal? maxPrice,
       double? minRating,
       bool inStockOnly = false,
       int page = 1,
       int pageSize = 8)
        {
            string? userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 8 : pageSize;

            var options = new ProductSearchOptions
            {
                Keyword = search,
                CategoryId = categoryId,   
                Sort = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRating = minRating,
                InStockOnly = inStockOnly,
                Page = page,
                PageSize = pageSize,
                CurrentUserId = userId
            };

            var result = _productService.SearchProducts(options);

            var cartQuantities = GetCartQuantities(userId);

            foreach (var product in result.Products)
            {
                cartQuantities.TryGetValue(product.Id, out int qty);
                product.AvailableStock = Math.Max(0, product.Stock - qty);
            }

            var categories = _unitOfWork.Categories
                .GetAll()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();

            return View(new ProductListViewModel
            {
                Products = result.Products,
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,

                Search = search,
                CategoryId = categoryId, 
                Sort = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRating = minRating,
                InStockOnly = inStockOnly,

                Categories = categories
            });
        }



        private Dictionary<int, int> GetCartQuantities(string? userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                return _cartService.GetCart(userId).Items
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
            }

            return _sessionCart.GetCart()
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
        }


        [AllowAnonymous]
        public IActionResult Filter(
     string? search,
     int? categoryId,
     string? sort,
     decimal? minPrice,
     decimal? maxPrice,
     double? minRating,
     bool inStockOnly = false,
     int page = 1,
     int pageSize = 8)
        {
            string? userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var options = new ProductSearchOptions
            {
                Keyword = search,
                CategoryId = categoryId,
                Sort = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRating = minRating,
                InStockOnly = false, 
                Page = page,
                PageSize = pageSize,
                CurrentUserId = userId
            };

            var searchResult = _productService.SearchProducts(options);

            var cartQuantities = GetCartQuantities(userId);

            foreach (var product in searchResult.Products)
            {
                cartQuantities.TryGetValue(product.Id, out int qty);
                product.AvailableStock = Math.Max(0, product.Stock - qty);
            }

            if (inStockOnly)
            {
                searchResult.Products = searchResult.Products
                    .Where(p => p.AvailableStock > 0)
                    .ToList();

                searchResult.TotalCount = searchResult.Products.Count();
            }

            return PartialView("_ProductsGrid", new ProductListViewModel
            {
                Products = searchResult.Products,
                TotalCount = searchResult.TotalCount,
                PageSize = searchResult.PageSize,
                CurrentPage = searchResult.CurrentPage
            });
        }


        [AllowAnonymous]
        public IActionResult AutoComplete(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(Array.Empty<object>());

            string? userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var items = _productService.AutoComplete(q, userId)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    category = p.CategoryName,
                    price = p.Price,
                    image = p.ImageUrl,
                    averageRating = p.AverageRating,
                    ratingCount = p.RatingCount,
                    isInWishlist = p.IsInWishlist
                });

            return Json(items);
        }

        public IActionResult Create()
        {
            return View(new ProductFormViewModel
            {
                Categories = GetCategorySelectList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            model.Categories = GetCategorySelectList();
            if (!ModelState.IsValid) return View(model);

            string? image = null;
            string? thumb = null;

            if (model.ImageFile != null)
                (image, thumb) = await _imageService.ProcessImageAsync(model.ImageFile, "products");

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                Stock = model.Stock,
                CategoryId = model.CategoryId,
                ImageUrl = image,
                ThumbnailUrl = thumb
            };

            _unitOfWork.Products.Add(product);
            _unitOfWork.Complete();

            await _logger.LogAsync(_userManager.GetUserId(User),
                "CreateProduct", $"Product: {product.Name} (ID: {product.Id})");

            _cache.Remove("products_page_1");
            TempData["Success"] = "Product created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var product = _unitOfWork.Products.GetProductWithCategoryById(id);
            if (product == null) return RedirectToAction(nameof(Index));

            var dto = _mapper.Map<ProductDto>(product);

            string? userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var cartQuantities = GetCartQuantities(userId);
            cartQuantities.TryGetValue(id, out int qty);


            dto.AvailableStock = Math.Max(0, dto.Stock - qty);

            return View(dto);
        }

        public IActionResult Edit(int id, string? returnUrl = null)
        {
            var product = _unitOfWork.Products.GetById(id);
            if (product == null) return RedirectToAction(nameof(Index));

            var vm = _mapper.Map<ProductFormViewModel>(_mapper.Map<ProductDto>(product));
            vm.ExistingImageUrl = product.ImageUrl;
            vm.Categories = GetCategorySelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormViewModel model, string? returnUrl = null)
        {
            model.Categories = GetCategorySelectList();
            if (!ModelState.IsValid) return View(model);

            var product = _unitOfWork.Products.GetById(model.Id);
            if (product == null) return RedirectToAction(nameof(Index));

            if (model.RemoveImage)
            {
                _imageService.DeleteImage(product.ImageUrl, "products");
                _imageService.DeleteImage(product.ThumbnailUrl, "products");
                product.ImageUrl = null;
                product.ThumbnailUrl = null;
            }
            else if (model.ImageFile != null)
            {
                _imageService.DeleteImage(product.ImageUrl, "products");
                _imageService.DeleteImage(product.ThumbnailUrl, "products");
                (product.ImageUrl, product.ThumbnailUrl) =
                    await _imageService.ProcessImageAsync(model.ImageFile, "products");
            }

            product.Name = model.Name;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.CategoryId = model.CategoryId;

            _unitOfWork.Products.Update(product);
            _unitOfWork.Complete();

            await _logger.LogAsync(_userManager.GetUserId(User),
                "EditProduct", $"Updated Product: {product.Name} (ID: {product.Id})");

            _cache.Remove("products_page_1");
            TempData["Success"] = "Product updated successfully!";
            return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id, string? returnUrl = null)
        {
            var product = _unitOfWork.Products.GetProductWithCategoryById(id);
            if (product == null) return RedirectToAction(nameof(Index));
            ViewBag.ReturnUrl = returnUrl;
            return View(_mapper.Map<ProductDto>(product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnUrl = null)
        {
            var product = _unitOfWork.Products.GetById(id);
            if (product == null) return RedirectToAction(nameof(Index));

            _imageService.DeleteImage(product.ImageUrl, "products");
            _unitOfWork.Products.Delete(product);
            _unitOfWork.Complete();

            await _logger.LogAsync(_userManager.GetUserId(User),
                "DeleteProduct", $"Product: {product.Name} (ID: {product.Id})");

            _cache.Remove("products_page_1");
            TempData["Success"] = "Product deleted!";
            return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetCategorySelectList()
        {
            return _unitOfWork.Categories.GetAll()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();
        }
    }
}
