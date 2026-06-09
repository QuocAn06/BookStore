using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Data
{
    public static class CatalogSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Idempotent: bỏ qua nếu đã có category
            if (await context.Categories.AnyAsync())
                return;

            // 1) Category trước — KHÔNG gán Id (để SQL Server Identity tự sinh)
            var fiction = new Category
            {
                Name = "Fiction",
                Description = "Novels and stories"
            };
            var science = new Category
            {
                Name = "Science",
                Description = "Science and nature"
            };
            var technology = new Category
            {
                Name = "Technology",
                Description = "Programming and IT"
            };

            context.Categories.AddRange(fiction, science, technology);
            await context.SaveChangesAsync(); // Sau bước này: fiction.Id, science.Id, technology.Id có giá trị

            // 2) Book — dùng CategoryId từ entity đã lưu (không gán Id cho Book)
            var books = new List<Book>
            {
                new()
                {
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    Price = 12.99m,
                    Stock = 50,
                    CategoryId = fiction.Id,
                    ImageUrl = "/images/books/gatsby.jpg"
                },
                new()
                {
                    Title = "A Brief History of Time",
                    Author = "Stephen Hawking",
                    Price = 15.50m,
                    Stock = 30,
                    CategoryId = science.Id,
                    ImageUrl = "/images/books/brief-history.jpg"
                },
                new()
                {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Price = 29.99m,
                    Stock = 40,
                    CategoryId = technology.Id,
                    ImageUrl = "/images/books/clean-code.jpg"
                },
                new()
                {
                    Title = "1984",
                    Author = "George Orwell",
                    Price = 10.99m,
                    Stock = 25,
                    CategoryId = fiction.Id
                }
            };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }
    }
}