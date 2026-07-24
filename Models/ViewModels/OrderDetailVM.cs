namespace BookStore.Models.ViewModels
{
    public class OrderDetailVM
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public IReadOnlyList<OrderDetailLineVM> Lines { get; set; } = Array.Empty<OrderDetailLineVM>();
    }
}