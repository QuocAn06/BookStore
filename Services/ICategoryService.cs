using BookStore.Models;

namespace BookStore.Services
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task<DeleteResult> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetBookCountAsync(int categoryId);
    }
}