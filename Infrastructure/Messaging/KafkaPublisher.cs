using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserReportService.Application.Interfaces;

namespace UserReportService.Infrastructure.Messaging;

public class KafkaPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaPublisher> _logger;

    public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
    {
        _logger = logger;
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var value = JsonSerializer.Serialize(message, options);
            var kafkaMessage = new Message<Null, string> { Value = value };
            
            var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
            _logger.LogInformation("Đã publish sự kiện lên Kafka topic {Topic} tại partition/offset {PartitionOffset}", topic, deliveryResult.TopicPartitionOffset);
        }
        catch (ProduceException<Null, string> e)
        {
            _logger.LogError(e, "Lỗi khi publish sự kiện lên Kafka topic {Topic}: {Reason}", topic, e.Error.Reason);
            throw; // Quăng lỗi để bảo đảm tính nhất quán của request (hoặc có thể bắt lỗi tùy chiến lược)
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
