using UserReportService.Domain.Entities;

namespace UserReportService.Domain.Interfaces;

public interface IReportRepository
{
    // Daily Revenue
    Task<DailyRevenue?> GetRevenueByDateAsync(DateTime date);
    Task AddDailyRevenueAsync(DailyRevenue dailyRevenue);
    void UpdateDailyRevenue(DailyRevenue dailyRevenue);
    Task<IEnumerable<DailyRevenue>> GetRevenueBetweenDatesAsync(DateTime fromDate, DateTime toDate);
    Task<decimal> GetRevenueSumAsync(DateTime fromDate, DateTime toDate);
    Task<int> GetOrdersCountAsync(DateTime fromDate, DateTime toDate);

    // Products Sold
    Task<ProductSold?> GetProductSoldByIdAndDateAsync(Guid productId, DateTime date);
    Task AddProductSoldAsync(ProductSold productSold);
    void UpdateProductSold(ProductSold productSold);
    Task<IEnumerable<ProductSold>> GetTopProductsAsync(int top, DateTime? fromDate, DateTime? toDate);

    // Customers Spent
    Task<CustomerSpent?> GetCustomerSpentByIdAsync(Guid customerId);
    Task AddCustomerSpentAsync(CustomerSpent customerSpent);
    void UpdateCustomerSpent(CustomerSpent customerSpent);
    Task<IEnumerable<CustomerSpent>> GetTopCustomersAsync(int top);
}
