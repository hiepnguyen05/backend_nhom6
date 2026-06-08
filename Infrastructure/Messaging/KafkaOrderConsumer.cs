using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserReportService.Application.DTOs;
using UserReportService.Application.Features.Reports.Commands;

namespace UserReportService.Infrastructure.Messaging;

/// <summary>
/// Kafka Consumer lắng nghe sự kiện đơn hàng mới từ Order Service (Nhóm 2).
/// Thay vì xử lý logic nghiệp vụ trực tiếp, consumer gửi Command qua MediatR
/// để tuân thủ đúng mô hình CQRS: logic nghiệp vụ nằm ở tầng Application.
/// </summary>
public class KafkaOrderConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaOrderConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _topic;

    public KafkaOrderConsumer(IConfiguration configuration, ILogger<KafkaOrderConsumer> logger, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _topic = "order.created"; // Could read from config
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = _configuration["Kafka:GroupId"] ?? "user-report-group",
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);
        _logger.LogInformation("Subscribed to topic {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult != null)
                    {
                        var message = consumeResult.Message.Value;
                        _logger.LogInformation("Received message: {Message}", message);

                        await ProcessOrderMessageAsync(message, stoppingToken);

                        consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Consume error: {Reason}", e.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Closing consumer.");
            consumer.Close();
        }
    }

    private async Task ProcessOrderMessageAsync(string message, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message, options);

        if (orderEvent == null) return;

        // Gửi Command qua MediatR — logic nghiệp vụ và cache invalidation
        // được xử lý tập trung tại ProcessOrderEventCommandHandler (Application layer)
        await mediator.Send(new ProcessOrderEventCommand(orderEvent), cancellationToken);
    }
}
