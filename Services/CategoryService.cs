using BookStore.Data;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<DeleteResult> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category is null)
                return DeleteResult.NotFoundResult();

            var hasBooks = await _context.Books.AnyAsync(b => b.CategoryId == id);
            if (hasBooks)
                return DeleteResult.Fail("Cannot delete a category that contains books.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return DeleteResult.Ok();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<int> GetBookCountAsync(int categoryId)
        {
            return await _context.Books.CountAsync(b => b.CategoryId == categoryId);
        }
    }
}
