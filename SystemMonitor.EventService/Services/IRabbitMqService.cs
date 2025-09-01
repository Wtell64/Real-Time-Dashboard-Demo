namespace SystemMonitor.EventService.Services;

public interface IRabbitMqService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task PublishAsync(string exchange, string routingKey, string message);
}