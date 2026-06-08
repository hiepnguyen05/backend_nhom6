using Microsoft.EntityFrameworkCore;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Interfaces;
using UserReportService.Infrastructure.Data;

namespace UserReportService.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly UserDbContext _context;

    public ReportRepository(UserDbContext context)
    {
        _context = context;
    }

    // Daily Revenue
    public async Task<DailyRevenue?> GetRevenueByDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return await _context.DailyRevenues.FirstOrDefaultAsync(r => r.Date == targetDate);
    }

    public async Task AddDailyRevenueAsync(DailyRevenue dailyRevenue)
    {
        dailyRevenue.Date = dailyRevenue.Date.Date;
        await _context.DailyRevenues.AddAsync(dailyRevenue);
    }

    public void UpdateDailyRevenue(DailyRevenue dailyRevenue)
    {
        dailyRevenue.Date = dailyRevenue.Date.Date;
        _context.DailyRevenues.Update(dailyRevenue);
    }

    public async Task<IEnumerable<DailyRevenue>> GetRevenueBetweenDatesAsync(DateTime fromDate, DateTime toDate)
    {
        var start = fromDate.Date;
        var end = toDate.Date;
        return await _context.DailyRevenues
            .Where(r => r.Date >= start && r.Date <= end)
            .OrderBy(r => r.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetRevenueSumAsync(DateTime fromDate, DateTime toDate)
    {
        var start = fromDate.Date;
        var end = toDate.Date;
        return await _context.DailyRevenues
            .Where(r => r.Date >= start && r.Date <= end)
            .SumAsync(r => r.TotalRevenue);
    }

    public async Task<int> GetOrdersCountAsync(DateTime fromDate, DateTime toDate)
    {
        var start = fromDate.Date;
        var end = toDate.Date;
        return await _context.DailyRevenues
            .Where(r => r.Date >= start && r.Date <= end)
            .SumAsync(r => r.TotalOrders);
    }

    // Products Sold
    public async Task<ProductSold?> GetProductSoldByIdAndDateAsync(Guid productId, DateTime date)
    {
        var targetDate = date.Date;
        return await _context.ProductsSold.FirstOrDefaultAsync(p => p.ProductId == productId && p.Date == targetDate);
    }

    public async Task AddProductSoldAsync(ProductSold productSold)
    {
        productSold.Date = productSold.Date.Date;
        await _context.ProductsSold.AddAsync(productSold);
    }

    public void UpdateProductSold(ProductSold productSold)
    {
        productSold.Date = productSold.Date.Date;
        _context.ProductsSold.Update(productSold);
    }

    public async Task<IEnumerable<ProductSold>> GetTopProductsAsync(int top, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.ProductsSold.AsQueryable();

        if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            query = query.Where(p => p.Date >= start);
        }

        if (toDate.HasValue)
        {
            var end = toDate.Value.Date;
            query = query.Where(p => p.Date <= end);
        }

        return await query
            .GroupBy(p => new { p.ProductId, p.ProductName })
            .Select(g => new ProductSold
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                TotalQuantitySold = g.Sum(p => p.TotalQuantitySold),
                TotalRevenue = g.Sum(p => p.TotalRevenue),
                Date = fromDate ?? DateTime.MinValue
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(top)
            .ToListAsync();
    }

    // Customers Spent
    public async Task<CustomerSpent?> GetCustomerSpentByIdAsync(Guid customerId)
    {
        return await _context.CustomersSpent.FindAsync(customerId);
    }

    public async Task AddCustomerSpentAsync(CustomerSpent customerSpent)
    {
        await _context.CustomersSpent.AddAsync(customerSpent);
    }

    public void UpdateCustomerSpent(CustomerSpent customerSpent)
    {
        _context.CustomersSpent.Update(customerSpent);
    }

    public async Task<IEnumerable<CustomerSpent>> GetTopCustomersAsync(int top)
    {
        return await _context.CustomersSpent
            .OrderByDescending(c => c.TotalSpent)
            .Take(top)
            .ToListAsync();
    }
}
