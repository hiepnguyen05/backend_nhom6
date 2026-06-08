using UserReportService.Domain.Entities;

namespace UserReportService.Domain.Interfaces;

public interface IOrderReceiptRepository
{
    Task<OrderReceipt?> GetByIdAsync(Guid orderId);
    Task AddAsync(OrderReceipt receipt);
}
