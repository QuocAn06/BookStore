namespace BookStore.Models.ViewModels
{
    public class CategoryDeleteVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BookCount { get; set; }
        public bool CanDelete => BookCount == 0;
    }
}
