using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductSearchResult SearchProducts(ProductSearchOptions options)
        {
            var baseQuery = _unitOfWork.Products
                .Query()
                .Include(p => p.Category)
                .AsNoTracking();

            var query =
                from p in baseQuery
                join r in _unitOfWork.Ratings.Query() on p.Id equals r.ProductId into ratingGroup
                select new
                {
                    Product = p,
                    Ratings = ratingGroup
                };

            if (!string.IsNullOrWhiteSpace(options.Keyword))
            {
                var term = options.Keyword.Trim();
                query = query.Where(x =>
                    x.Product.Name.Contains(term) ||
                    (x.Product.Category != null && x.Product.Category.Name.Contains(term)));
            }

            if (options.CategoryId.HasValue && options.CategoryId.Value > 0)
            {
                query = query.Where(x => x.Product.CategoryId == options.CategoryId.Value);
            }

            if (options.MinPrice.HasValue)
                query = query.Where(x => x.Product.Price >= options.MinPrice.Value);

            if (options.MaxPrice.HasValue)
                query = query.Where(x => x.Product.Price <= options.MaxPrice.Value);

            if (options.MinRating.HasValue)
            {
                query = query.Where(x =>
                    x.Ratings.Any()
                        ? x.Ratings.Average(r => r.Stars) >= options.MinRating.Value
                        : 0 >= options.MinRating.Value);
            }

            if (options.InStockOnly)
                query = query.Where(x => x.Product.Stock > 0);

            query = options.Sort switch
            {
                "price_asc" => query.OrderBy(x => x.Product.Price),
                "price_desc" => query.OrderByDescending(x => x.Product.Price),

                "name_asc" => query.OrderBy(x => x.Product.Name),
                "name_desc" => query.OrderByDescending(x => x.Product.Name),

                "rating_desc" => query.OrderByDescending(x =>
                    x.Ratings.Any() ? x.Ratings.Average(r => r.Stars) : 0),

                "popular" => query.OrderByDescending(x => x.Ratings.Count()),

                _ => query.OrderByDescending(x => x.Product.Id)
            };

            int totalItems = query.Count();
            int page = options.Page <= 0 ? 1 : options.Page;
            int pageSize = options.PageSize <= 0 ? 8 : options.PageSize;

            var result = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = result.Select(x => new ProductSearchResultItem
            {
                Id = x.Product.Id,
                Name = x.Product.Name,
                CategoryName = x.Product.Category?.Name,
                Price = x.Product.Price,
                Stock = x.Product.Stock,

                ImageUrl = x.Product.ImageUrl,
                ThumbnailUrl = x.Product.ThumbnailUrl,

                AverageRating = x.Ratings.Any() ? x.Ratings.Average(r => r.Stars) : 0,
                RatingCount = x.Ratings.Count(),

                IsInWishlist = options.CurrentUserId != null &&
                               _unitOfWork.Wishlists.Exists(options.CurrentUserId, x.Product.Id)

            }).ToList();

            return new ProductSearchResult
            {
                Products = items,
                TotalCount = totalItems,
                PageSize = pageSize,
                CurrentPage = page
            };
        }


        public IEnumerable<ProductSearchResultItem> AutoComplete(string keyword, string? userId)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Enumerable.Empty<ProductSearchResultItem>();

            keyword = keyword.Trim();

            var baseQuery = _unitOfWork.Products
                .Query()
                .Include(p => p.Category)
                .Where(p =>
                    p.Name.Contains(keyword) ||
                    (p.Category != null && p.Category.Name.Contains(keyword)))
                .OrderByDescending(p => p.Id)
                .Take(10);

            var query =
                from p in baseQuery
                join r in _unitOfWork.Ratings.Query() on p.Id equals r.ProductId into ratingGroup
                select new
                {
                    Product = p,
                    Ratings = ratingGroup
                };

            return query
                .ToList()
                .Select(x => new ProductSearchResultItem
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    CategoryName = x.Product.Category?.Name,
                    Price = x.Product.Price,

                    ImageUrl = x.Product.ImageUrl,
                    ThumbnailUrl = x.Product.ThumbnailUrl,
                    Stock = x.Product.Stock,

                    AverageRating = x.Ratings.Any() ? x.Ratings.Average(r => r.Stars) : 0,
                    RatingCount = x.Ratings.Count(),

                    IsInWishlist = userId != null &&
                                   _unitOfWork.Wishlists.Exists(userId, x.Product.Id)
                });
        }
    }
}
