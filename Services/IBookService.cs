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
        Task<bool> DeleteAsync(int id);
        Task<AdminBooksIndexVM> SearchAsync(string? title, int? categoryId);
        Task<BookDetailVM?> GetDetailVmAsync(int id);
        Task<BookCatalogVM> GetCatalogAsync(int page, int pageSize = 12);
    }
}