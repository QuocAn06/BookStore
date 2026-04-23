using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150, ErrorMessage = "Author cannot exceed 150 characters.")]
        public string Author { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string? ImageUrl { get; set; }

        // FK
        [Display(Name = "Category")]
        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }
        
        // Many Books belong to one Category
        public Category? Category { get; set; }
    }
}
