using Microsoft.AspNetCore.Identity;

namespace BookStore.Models
{
    public class ApplicationUser: IdentityUser
    {
        // One user can place many orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
