using MediatR;
using UserReportService.Application.Interfaces;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Interfaces;

namespace UserReportService.Application.Features.Reports.Queries;

// ======================== GET DAILY REVENUE (có Redis Cache) ========================
public record GetDailyRevenueQuery(DateTime StartDate, DateTime EndDate) : IRequest<IEnumerable<DailyRevenue>>;

public class GetDailyRevenueQueryHandler : IRequestHandler<GetDailyRevenueQuery, IEnumerable<DailyRevenue>>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICacheService _cacheService;

    public GetDailyRevenueQueryHandler(IReportRepository reportRepository, ICacheService cacheService)
    {
        _reportRepository = reportRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<DailyRevenue>> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
    {
        // Tạo cache key dựa trên tham số truy vấn
        var cacheKey = $"reports:daily-revenue:{request.StartDate:yyyyMMdd}-{request.EndDate:yyyyMMdd}";

        // 1. Kiểm tra cache Redis trước (Read Model)
        var cached = await _cacheService.GetAsync<IEnumerable<DailyRevenue>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // 2. Cache miss → truy vấn SQL Server
        var result = await _reportRepository.GetRevenueBetweenDatesAsync(request.StartDate, request.EndDate);

        // 3. Lưu kết quả vào Redis Cache (TTL: 10 phút)
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);

        return result;
    }
}

// ======================== GET TOP SELLING PRODUCTS (có Redis Cache) ========================
public record GetTopProductsQuery(int TopN, DateTime? StartDate = null, DateTime? EndDate = null) : IRequest<IEnumerable<ProductSold>>;

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, IEnumerable<ProductSold>>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICacheService _cacheService;

    public GetTopProductsQueryHandler(IReportRepository reportRepository, ICacheService cacheService)
    {
        _reportRepository = reportRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<ProductSold>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"reports:top-products:{request.TopN}-{request.StartDate?.ToString("yyyyMMdd") ?? "all"}-{request.EndDate?.ToString("yyyyMMdd") ?? "all"}";

        var cached = await _cacheService.GetAsync<IEnumerable<ProductSold>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var result = await _reportRepository.GetTopProductsAsync(request.TopN, request.StartDate, request.EndDate);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);

        return result;
    }
}

// ======================== GET TOP CUSTOMERS (có Redis Cache) ========================
public record GetTopCustomersQuery(int TopN) : IRequest<IEnumerable<CustomerSpent>>;

public class GetTopCustomersQueryHandler : IRequestHandler<GetTopCustomersQuery, IEnumerable<CustomerSpent>>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICacheService _cacheService;

    public GetTopCustomersQueryHandler(IReportRepository reportRepository, ICacheService cacheService)
    {
        _reportRepository = reportRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<CustomerSpent>> Handle(GetTopCustomersQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"reports:top-customers:{request.TopN}";

        var cached = await _cacheService.GetAsync<IEnumerable<CustomerSpent>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var result = await _reportRepository.GetTopCustomersAsync(request.TopN);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);

        return result;
    }
}
