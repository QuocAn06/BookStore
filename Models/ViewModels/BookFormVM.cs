using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.ViewModels
{
    public class BookFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // Save existing image path for Edit screen
        public string? ExistingImageUrl { get; set; }

        // New uploaded file (Create/Edit)
        public IFormFile? ImageFile { get; set; }

        // Dropdown data source
        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();


    }
}
