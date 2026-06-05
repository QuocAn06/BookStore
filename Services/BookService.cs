using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IReadOnlyList<Book>> GetAllWithCategoryAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book?> GetByIdWithCategoryAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IReadOnlyList<SelectListItem>> GetCategorySelectListAsync(int? selectedId = null)
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = selectedId.HasValue && c.Id == selectedId.Value
                })
                .ToListAsync();
        }

        public async Task CreateAsync(Book book, IFormFile? imageFile)
        {
            if (imageFile != null)
            {
                book.ImageUrl = await SaveImageAsync(imageFile);
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Book book, IFormFile? imageFile)
        {
            var existing = await _context.Books.FindAsync(book.Id);
            if (existing is null)
                return false;

            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.Price = book.Price;
            existing.Stock = book.Stock;
            existing.CategoryId = book.CategoryId;

            if (imageFile != null)
            {
                if (!string.IsNullOrWhiteSpace(existing.ImageUrl))
                {
                    DeleteImageFile(existing.ImageUrl);
                }
                existing.ImageUrl = await SaveImageAsync(imageFile);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book is null)
                return false;

            if (!string.IsNullOrWhiteSpace(book.ImageUrl))
            {
                DeleteImageFile(book.ImageUrl);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AdminBooksIndexVM> SearchAsync(string? title, int? categoryId)
        {
            IQueryable<Book> query = _context.Books
                .AsNoTracking()
                .Include(b => b.Category);
            if (!string.IsNullOrWhiteSpace(title))
            {
                var term = title.Trim();
                query = query.Where(b => b.Title.Contains(term));
            }
            if (categoryId is int cid && cid > 0)
            {
                query = query.Where(b => b.CategoryId == cid);
            }
            var books = await query
                .OrderBy(b => b.Title)
                .ToListAsync();
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = categoryId == c.Id
                })
                .ToListAsync();
            categories.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "All categories",
                Selected = categoryId is null or 0
            });
            return new AdminBooksIndexVM
            {
                Books = books,
                TitleSearch = title,
                CategoryId = categoryId,
                CategoryOptions = categories
            };
        }

        public async Task<BookDetailVM?> GetDetailVmAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (book is null)
                return null;
            List<Book>? relatedBooksInCategory = null;
            if (book.CategoryId != 0)
            {
                relatedBooksInCategory = await _context.Books
                    .AsNoTracking()
                    .Where(b => b.CategoryId == book.CategoryId && b.Id != book.Id)
                    .OrderBy(b => b.Title)
                    .Take(5)
                    .ToListAsync();
            }
            return new BookDetailVM
            {
                Book = book,
                RelatedBooksInCategory = relatedBooksInCategory
            };
        }

        public async Task<BookCatalogVM> GetCatalogAsync(int page, int pageSize = 12)
        {
            if (page < 1) page = 1;

            var totalCount = await _context.Books
                .AsNoTracking()
                .CountAsync();

            var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b => b.Stock > 0)
                .OrderByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BookCatalogVM
            {
                Books = books,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/books/{fileName}";
        }

        private void DeleteImageFile(string imageUrl)
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}