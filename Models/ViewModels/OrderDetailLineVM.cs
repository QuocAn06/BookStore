namespace BookStore.Models.ViewModels
{
    public class OrderDetailLineVM
    {
        public string BookTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}