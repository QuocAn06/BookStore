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
    }
}
