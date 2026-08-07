using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Services
{
    public interface IBookService
    {
        Task<IReadOnlyList<Book>> GetAllWithCategoryAsync();
        Task<Book?> GetByIdAsync(int id);
        Task<Book?> GetByIdWithCategoryAsync(int id);
        Task<IReadOnlyList<SelectListItem>> GetCategorySelectListAsync(int? selectedId = null);
        Task CreateAsync(Book book, IFormFile? imageFile);
        Task<bool> UpdateAsync(Book book, IFormFile? imageFile);
        Task<DeleteResult> DeleteAsync(int id);
        Task<bool> IsReferencedByOrdersAsync(int bookId);
        Task<AdminBooksIndexVM> SearchAsync(string? title, int? categoryId);
        Task<BookDetailVM?> GetDetailVmAsync(int id);
        Task<BookCatalogVM> GetCatalogAsync(
            int page,
            string? search = null,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int pageSize = 12);
    }
}