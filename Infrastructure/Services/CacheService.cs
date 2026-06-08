using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UserReportService.Application.Interfaces;

namespace UserReportService.Infrastructure.Services;

/// <summary>
/// Triển khai ICacheService sử dụng Redis (IDistributedCache).
/// Chỉ cache dữ liệu báo cáo (Dashboard), KHÔNG cache thông tin bảo mật.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);
            if (cachedData == null)
            {
                _logger.LogDebug("Cache MISS cho key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache HIT cho key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            // Nếu Redis bị lỗi, log cảnh báo nhưng không crash ứng dụng.
            // Ứng dụng sẽ fallback sang đọc trực tiếp từ SQL Server.
            _logger.LogWarning(ex, "Lỗi khi đọc cache cho key: {Key}. Fallback sang SQL Server.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
            _logger.LogDebug("Đã lưu cache cho key: {Key}, TTL: {TTL}", key, expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi ghi cache cho key: {Key}. Bỏ qua.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Đã xóa cache cho key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi xóa cache cho key: {Key}. Bỏ qua.", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // IDistributedCache không hỗ trợ xóa theo prefix trực tiếp.
        // Với Redis, ta sử dụng cách tiếp cận đơn giản: xóa các key cụ thể đã biết trước.
        // Đây là các cache key cố định cho báo cáo.
        var reportKeys = new[]
        {
            $"{prefix}:daily-revenue",
            $"{prefix}:top-products",
            $"{prefix}:top-customers"
        };

        foreach (var key in reportKeys)
        {
            await RemoveAsync(key, cancellationToken);
        }

        _logger.LogInformation("Đã xóa tất cả cache báo cáo có prefix: {Prefix}", prefix);
    }
}
