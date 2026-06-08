using MediatR;
using Microsoft.Extensions.Logging;
using UserReportService.Application.DTOs;
using UserReportService.Application.Interfaces;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Interfaces;

namespace UserReportService.Application.Features.Reports.Commands;

/// <summary>
/// Command xử lý sự kiện đơn hàng mới từ Kafka.
/// Sau khi cập nhật SQL Server, sẽ invalidate (xóa) toàn bộ cache báo cáo trong Redis
/// để đảm bảo lần đọc tiếp theo sẽ lấy dữ liệu mới nhất.
/// </summary>
public record ProcessOrderEventCommand(OrderCreatedEvent OrderEvent) : IRequest<Unit>;

public class ProcessOrderEventCommandHandler : IRequestHandler<ProcessOrderEventCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProcessOrderEventCommandHandler> _logger;

    public ProcessOrderEventCommandHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ProcessOrderEventCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ProcessOrderEventCommand request, CancellationToken cancellationToken)
    {
        var orderEvent = request.OrderEvent;

        // Kiểm tra trùng lặp (Idempotency)
        var existingReceipt = await _unitOfWork.OrderReceipts.GetByIdAsync(orderEvent.OrderId);
        if (existingReceipt != null)
        {
            _logger.LogInformation("Order {OrderId} đã được xử lý trước đó. Bỏ qua.", orderEvent.OrderId);
            return Unit.Value;
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 1. Đánh dấu đã xử lý
            var receipt = new OrderReceipt
            {
                OrderId = orderEvent.OrderId,
                ProcessedAt = DateTime.UtcNow
            };
            await _unitOfWork.OrderReceipts.AddAsync(receipt);

            // 2. Cập nhật CustomerSpent
            var customerSpent = await _unitOfWork.Reports.GetCustomerSpentByIdAsync(orderEvent.CustomerId);
            if (customerSpent == null)
            {
                customerSpent = new CustomerSpent
                {
                    CustomerId = orderEvent.CustomerId,
                    CustomerName = orderEvent.CustomerName,
                    TotalSpent = orderEvent.TotalAmount,
                    TotalOrders = 1,
                    LastOrderDate = orderEvent.CreatedAt
                };
                await _unitOfWork.Reports.AddCustomerSpentAsync(customerSpent);
            }
            else
            {
                customerSpent.TotalSpent += orderEvent.TotalAmount;
                customerSpent.TotalOrders += 1;
                customerSpent.LastOrderDate = orderEvent.CreatedAt;
                _unitOfWork.Reports.UpdateCustomerSpent(customerSpent);
            }

            // 3. Cập nhật DailyRevenue
            var dateOnly = orderEvent.CreatedAt.Date;
            var dailyRevenue = await _unitOfWork.Reports.GetRevenueByDateAsync(dateOnly);
            if (dailyRevenue == null)
            {
                dailyRevenue = new DailyRevenue
                {
                    Date = dateOnly,
                    TotalRevenue = orderEvent.TotalAmount,
                    TotalOrders = 1
                };
                await _unitOfWork.Reports.AddDailyRevenueAsync(dailyRevenue);
            }
            else
            {
                dailyRevenue.TotalRevenue += orderEvent.TotalAmount;
                dailyRevenue.TotalOrders += 1;
                _unitOfWork.Reports.UpdateDailyRevenue(dailyRevenue);
            }

            // 4. Cập nhật ProductSold cho từng item
            foreach (var item in orderEvent.Items)
            {
                var productSold = await _unitOfWork.Reports.GetProductSoldByIdAndDateAsync(item.ProductId, dateOnly);
                if (productSold == null)
                {
                    productSold = new ProductSold
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Date = dateOnly,
                        TotalQuantitySold = item.Quantity,
                        TotalRevenue = item.Quantity * item.UnitPrice
                    };
                    await _unitOfWork.Reports.AddProductSoldAsync(productSold);
                }
                else
                {
                    productSold.TotalQuantitySold += item.Quantity;
                    productSold.TotalRevenue += item.Quantity * item.UnitPrice;
                    _unitOfWork.Reports.UpdateProductSold(productSold);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 5. INVALIDATE CACHE — Xóa toàn bộ cache báo cáo cũ trong Redis
            //    để lần đọc tiếp theo sẽ lấy dữ liệu mới nhất từ SQL Server.
            await _cacheService.RemoveByPrefixAsync("reports", cancellationToken);

            _logger.LogInformation("Đã xử lý thành công order {OrderId} và invalidate cache báo cáo", orderEvent.OrderId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Lỗi khi xử lý order {OrderId}", orderEvent.OrderId);
            throw;
        }

        return Unit.Value;
    }
}
