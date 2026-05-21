using BookStore.Infrastructure;
using BookStore.Models;
using System.Text.Json;

namespace BookStore.Services
{
    public class CartSessionService: ICartSessionService
    {
        private readonly IHttpContextAccessor _http;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CartSessionService(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ISession Session => _http.HttpContext!.Session;

        public Cart GetCart()
        {
            var json = Session.GetString(SessionKeys.Cart);          
            if (string.IsNullOrEmpty(json))
                return new Cart();

            return JsonSerializer.Deserialize<Cart>(json, JsonOptions) ?? new Cart();
        }

        public void SaveCart(Cart cart)
        {
            var json = JsonSerializer.Serialize(cart, JsonOptions);
            Session.SetString(SessionKeys.Cart, json);
        }

        public void AddToCart(int productId, string name, decimal unitPrice, int quantity)
        {
            if (quantity <= 0) return;

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Name = name,
                    UnitPrice = unitPrice,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            SaveCart(cart);
        }

        public void Remove(int productId)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(x => x.ProductId == productId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item is null) return;
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            SaveCart(cart);
        }

        public void Clear()
        {
            Session.Remove(SessionKeys.Cart);
        }

    }
}
