using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Data
{
    public static class IdentitySeed
    {
        // Dev-only defaults; change email/password for your environment
        private const string AdminEmail = "admin@bookstore.com";
        private const string AdminPassword = "Admin@123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Create Admin role if missing
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }

            // 2) Create admin user if missing
            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Failed to create admin user: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            // 3) Assign Admin role (also fixes an existing user without the role)
            if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }
    }
}