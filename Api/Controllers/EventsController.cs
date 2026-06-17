using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserReportService.Application.DTOs;
using UserReportService.Application.Features.Reports.Commands;

namespace UserReportService.Api.Controllers;

/// <summary>
/// Controller tiếp nhận event từ các Service khác thông qua API Gateway (HTTP POST).
/// Thay thế cho Kafka Consumer — các Service khác (VD: Order Service) sẽ gọi
/// HTTP POST tới endpoint này để thông báo sự kiện đơn hàng mới.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IMediator mediator, ILogger<EventsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Nhận event khi có đơn hàng mới được tạo từ Order Service.
    /// Logic nghiệp vụ (cập nhật báo cáo, invalidate cache) được xử lý
    /// bởi ProcessOrderEventCommandHandler thông qua MediatR (CQRS pattern).
    /// </summary>
    /// <param name="orderEvent">Thông tin đơn hàng mới</param>
    /// <returns>Kết quả xử lý</returns>
    [HttpPost("order-created")]
    public async Task<IActionResult> OrderCreated([FromBody] OrderCreatedEvent orderEvent)
    {
        _logger.LogInformation(
            "Nhận event đơn hàng mới từ API Gateway: OrderId={OrderId}, CustomerId={CustomerId}, TotalAmount={TotalAmount}",
            orderEvent.OrderId, orderEvent.CustomerId, orderEvent.TotalAmount);

        try
        {
            await _mediator.Send(new ProcessOrderEventCommand(orderEvent));

            _logger.LogInformation("Đã xử lý thành công event đơn hàng {OrderId}", orderEvent.OrderId);

            return Ok(new
            {
                success = true,
                message = $"Đã xử lý thành công event đơn hàng {orderEvent.OrderId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý event đơn hàng {OrderId}", orderEvent.OrderId);

            return StatusCode(500, new
            {
                success = false,
                message = $"Lỗi khi xử lý event đơn hàng: {ex.Message}"
            });
        }
    }
}
