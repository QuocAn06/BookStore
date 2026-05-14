namespace BookStore.Models.ViewModels
{
    public class BookDetailVM
    {
        public Book Book { get; set; } = null!;

        public string CategoryName => Book.Category?.Name ?? "N/A";
        public string AuthorName => string.IsNullOrWhiteSpace(Book.Author) ? "N/A" : Book.Author;
        public List<Book>? RelatedBooksInCategory { get; set; }
    }
}
