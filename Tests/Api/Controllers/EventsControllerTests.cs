using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserReportService.Api.Controllers;
using UserReportService.Application.DTOs;
using UserReportService.Application.Features.Reports.Commands;

namespace UserReportService.Tests.Api.Controllers;

public class EventsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<EventsController>> _mockLogger;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<EventsController>>();
        _controller = new EventsController(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task OrderCreated_ShouldReturnOk_WhenProcessingSucceeds()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 1000
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<ProcessOrderEventCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.OrderCreated(orderEvent);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockMediator.Verify(m => m.Send(It.Is<ProcessOrderEventCommand>(c => c.OrderEvent.OrderId == orderEvent.OrderId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OrderCreated_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 1000
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<ProcessOrderEventCommand>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.OrderCreated(orderEvent);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        _mockMediator.Verify(m => m.Send(It.IsAny<ProcessOrderEventCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
