using System.Threading;
using System.Threading.Tasks;

namespace UserReportService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
}
