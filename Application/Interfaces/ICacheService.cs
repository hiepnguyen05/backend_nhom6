namespace UserReportService.Application.Interfaces;

/// <summary>
/// Interface cho dịch vụ bộ nhớ đệm (Cache) - triển khai bởi Redis trong tầng Infrastructure.
/// Chỉ sử dụng cho dữ liệu báo cáo, KHÔNG cache dữ liệu nhạy cảm (User, Auth).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Lấy dữ liệu từ cache theo key.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu dữ liệu vào cache với thời gian sống (TTL) tùy chỉnh.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa dữ liệu cache theo key (Invalidate khi có dữ liệu mới từ API Gateway).
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tất cả cache có prefix nhất định (ví dụ: xóa toàn bộ cache báo cáo).
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
