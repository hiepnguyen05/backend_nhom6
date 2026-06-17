using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using UserReportService.Application.DTOs;
using UserReportService.Application.Features.Reports.Commands;
using UserReportService.Application.Interfaces;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Interfaces;

namespace UserReportService.Tests.Application.Features.Reports.Commands;

public class ProcessOrderEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<ProcessOrderEventCommandHandler>> _mockLogger;
    private readonly ProcessOrderEventCommandHandler _handler;

    public ProcessOrderEventCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ProcessOrderEventCommandHandler>>();

        _handler = new ProcessOrderEventCommandHandler(
            _mockUnitOfWork.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldSkipProcessing_WhenOrderAlreadyExists()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent { OrderId = Guid.NewGuid() };
        var command = new ProcessOrderEventCommand(orderEvent);

        _mockUnitOfWork.Setup(u => u.OrderReceipts.GetByIdAsync(orderEvent.OrderId))
            .ReturnsAsync(new OrderReceipt { OrderId = orderEvent.OrderId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockCacheService.Verify(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldProcessSuccessfully_WhenOrderIsNew()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            TotalAmount = 500,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100 }
            }
        };
        var command = new ProcessOrderEventCommand(orderEvent);

        _mockUnitOfWork.Setup(u => u.OrderReceipts.GetByIdAsync(orderEvent.OrderId))
            .ReturnsAsync((OrderReceipt?)null); // Order does not exist

        _mockUnitOfWork.Setup(u => u.Reports.GetCustomerSpentByIdAsync(orderEvent.CustomerId))
            .ReturnsAsync((CustomerSpent?)null);

        _mockUnitOfWork.Setup(u => u.Reports.GetRevenueByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((DailyRevenue?)null);

        _mockUnitOfWork.Setup(u => u.Reports.GetProductSoldByIdAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync((ProductSold?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.OrderReceipts.AddAsync(It.Is<OrderReceipt>(r => r.OrderId == orderEvent.OrderId)), Times.Once);
        _mockUnitOfWork.Verify(u => u.Reports.AddCustomerSpentAsync(It.IsAny<CustomerSpent>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Reports.AddDailyRevenueAsync(It.IsAny<DailyRevenue>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Reports.AddProductSoldAsync(It.IsAny<ProductSold>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

        _mockCacheService.Verify(c => c.RemoveByPrefixAsync("reports", It.IsAny<CancellationToken>()), Times.Once);
    }
}
