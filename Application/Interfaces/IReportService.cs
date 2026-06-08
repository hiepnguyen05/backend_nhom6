namespace UserReportService.Application.Interfaces;

public interface IReportService
{
    Task<dynamic> GetDailyRevenueAsync(DateTime startDate, DateTime endDate);
    Task<dynamic> GetTopSellingProductsAsync(int topN, DateTime? startDate = null, DateTime? endDate = null);
    Task<dynamic> GetTopCustomersAsync(int topN);
}
