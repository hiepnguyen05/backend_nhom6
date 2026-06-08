using UserReportService.Application.Interfaces;
using UserReportService.Domain.Interfaces;

namespace UserReportService.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<dynamic> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
    {
        return await _reportRepository.GetRevenueBetweenDatesAsync(startDate, endDate);
    }

    public async Task<dynamic> GetTopSellingProductsAsync(int topN, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _reportRepository.GetTopProductsAsync(topN, startDate, endDate);
    }

    public async Task<dynamic> GetTopCustomersAsync(int topN)
    {
        return await _reportRepository.GetTopCustomersAsync(topN);
    }
}
