namespace BookStore.Models.ViewModels
{
    public class BookDeleteVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsReferencedByOrders { get; set; }
        public bool CanDelete => !IsReferencedByOrders;
    }
}
