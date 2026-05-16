namespace BookStore.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal GrandTotal => Items.Sum(x => x.LineTotal);
    }
}
