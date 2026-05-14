using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Models.ViewModels
{
    public class AdminBooksIndexVM
    {
        public IReadOnlyList<Book> Books { get; set; } = Array.Empty<Book>();
        public string? TitleSearch { get; set; }
        public int? CategoryId { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; } =  Array.Empty<SelectListItem>();
    }
}
