using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        // Many OrderDetails belong to one Order
        public Order? Order { get; set; }

        // Many OrderDetails can reference one Book
        public Book? Book { get; set; }
    }
}
