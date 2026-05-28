using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.ViewModels
{
    public class BookFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150, ErrorMessage = "Author cannot exceed 150 characters.")]
        [Display(Name = "Author")]
        public string Author { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Book Image")]
        public IFormFile? ImageFile { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();
    }
}