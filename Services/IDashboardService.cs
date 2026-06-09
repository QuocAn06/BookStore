using BookStore.Models.ViewModels;

namespace BookStore.Services
{
    public interface IDashboardService
    {
        Task<AdminHomeBooksVM> GetDashboardAsync(int page, int pageSize = 10);
    }
}
