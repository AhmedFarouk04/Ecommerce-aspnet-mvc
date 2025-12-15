using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;

namespace ECommerce.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public CartDto GetCart(string userId)
        {
            var cart = _unitOfWork.Carts.GetByUserId(userId);
            if (cart == null)
                return new CartDto();

            return MapToDto(cart);
        }

        public void Clear(string userId)
        {
            var cart = _unitOfWork.Carts.GetByUserId(userId);
            if (cart == null) return;

            cart.Items.Clear();
            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Complete();
        }


        public void Add(string userId, int productId)
        {
            var cart = _unitOfWork.Carts.GetOrCreate(userId);
            var product = _unitOfWork.Products.GetById(productId);
            if (product == null) return;

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            int currentQty = item?.Quantity ?? 0;
            if (currentQty + 1 > product.Stock)
                return;

            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    PriceAtTime = product.Price
                });
            }
            else
            {
                item.Quantity++;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Complete();
        }

        public void Remove(string userId, int productId)
        {
            var cart = _unitOfWork.Carts.GetByUserId(userId);
            if (cart == null) return;

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            cart.Items.Remove(item);
            _unitOfWork.Complete();
        }

        public void MergeGuestCart(string userId, List<CartItemDto> guestItems)
        {
            if (!guestItems.Any()) return;

            var cart = _unitOfWork.Carts.GetOrCreate(userId);

            foreach (var g in guestItems)
            {
                var product = _unitOfWork.Products.GetById(g.ProductId);
                if (product == null) continue;

                var existing = cart.Items.FirstOrDefault(x => x.ProductId == g.ProductId);

                int allowedQty = product.Stock;

                if (existing == null)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = g.ProductId,
                        Quantity = Math.Min(g.Quantity, allowedQty),
                        PriceAtTime = g.Price
                    });
                }
                else
                {
                    existing.Quantity = Math.Min(existing.Quantity + g.Quantity, allowedQty);
                }
            }

            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Complete();
        }

        public CartItemUpdateResult UpdateQuantity(string userId, int productId, int delta)
        {
            var cart = _unitOfWork.Carts.GetByUserId(userId);
            if (cart == null) return Fail();

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return Fail();

            var product = _unitOfWork.Products.GetById(productId);
            if (product == null) return Fail();

            int newQty = item.Quantity + delta;

            // REMOVE ITEM
            if (newQty <= 0)
            {
                cart.Items.Remove(item);
                _unitOfWork.Complete();
                return BuildResult(cart, productId, 0);
            }

            // 🔥 STOCK VALIDATION (CORRECT)
            if (newQty > product.Stock)
            {
                return new CartItemUpdateResult
                {
                    Success = false,
                    Message = $"Only {product.Stock} item(s) available",
                    Quantity = item.Quantity,
                    ItemTotal = item.Quantity * item.PriceAtTime,
                    Count = cart.Items.Sum(x => x.Quantity),
                    Total = cart.Items.Sum(x => x.Quantity * x.PriceAtTime)
                };
            }

            item.Quantity = newQty;
            _unitOfWork.Complete();

            return BuildResult(cart, productId, item.Quantity);
        }

        private CartItemUpdateResult BuildResult(Cart cart, int productId, int quantity)
        {
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            return new CartItemUpdateResult
            {
                Success = true,
                Quantity = quantity,
                ItemTotal = item != null ? item.Quantity * item.PriceAtTime : 0,
                Count = cart.Items.Sum(x => x.Quantity),
                Total = cart.Items.Sum(x => x.Quantity * x.PriceAtTime)
            };
        }

        private CartItemUpdateResult Fail()
        {
            return new CartItemUpdateResult { Success = false };
        }

        private CartDto MapToDto(Cart cart)
        {
            var productIds = cart.Items.Select(x => x.ProductId).ToList();

            var products = _unitOfWork.Products
                .GetAll()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionary(p => p.Id);

            var items = cart.Items
                .Select(i =>
                {
                    if (!products.TryGetValue(i.ProductId, out var p))
                        return null; 

                    return new CartItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = p.Name,
                        ImageUrl = p.ImageUrl,
                        Price = i.PriceAtTime,
                        Quantity = i.Quantity
                    };
                })
                .Where(x => x != null)
                .ToList();

            if (items.Count != cart.Items.Count)
            {
                cart.Items = cart.Items
                    .Where(i => products.ContainsKey(i.ProductId))
                    .ToList();

                _unitOfWork.Complete();
            }

            return new CartDto
            {
                Items = items
            };
        }

    }
}
