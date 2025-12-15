using ECommerce.Web.Helpers;
using ECommerce.Web.ViewModels;
using System.Globalization;
using System.Security.Claims;

namespace ECommerce.Web.Services
{
    public class SessionCartService
    {
        private readonly IHttpContextAccessor _http;


        public SessionCartService(IHttpContextAccessor http)
        {
            _http = http;
        }

        private string GetCartKey()
        {
            var context = _http.HttpContext;
            var user = context.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return $"Cart_{userId}";
            }

            return "Cart_Guest";
        }


        private ISession Session => _http.HttpContext.Session;

        public List<CartItem> GetCart()
        {
            var key = GetCartKey();
            return Session.GetObject<List<CartItem>>(key) ?? new List<CartItem>();
        }

        public void SaveCart(List<CartItem> cart)
        {
            var key = GetCartKey();
            Session.SetObject(key, cart);
        }


        public void ClearCart()
        {
            var key = GetCartKey();
            Session.Remove(key);
        }



        public int GetCount()
        {
            return GetCart().Sum(x => x.Quantity);
        }

        public decimal GetTotal()
        {
            return GetCart().Sum(x => x.Total);
        }


        public void ClampCartQuantities(int productId, int newStock)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            if (newStock <= 0)
            {
                cart.Remove(item);
            }
            else if (item.Quantity > newStock)
            {
                item.Quantity = newStock;
            }

            SaveCart(cart);
        }
        public UpdateQuantityResult UpdateQuantity(int productId, int quantity, int stock)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null)
            {
                return new UpdateQuantityResult
                {
                    Success = false,
                    Message = "Item not found"
                };
            }

            if (quantity < 1) quantity = 1;
            if (quantity > stock) quantity = stock;

            item.Quantity = quantity;
            SaveCart(cart);

            return new UpdateQuantityResult
            {
                Success = true,
                Message = "Quantity updated",
                Quantity = item.Quantity,
                ItemTotal = item.Total,
                Total = cart.Sum(x => x.Total),
                Count = cart.Sum(x => x.Quantity)
            };
        }

    }
}
