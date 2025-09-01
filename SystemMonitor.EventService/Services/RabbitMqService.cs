using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using SystemMonitor.DataAccess.Services;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.EventService.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "admin",
            Password = "admin123",
            Port = 5672
        };

        // Override with configuration if running in Docker
        var rabbitMqConnection = configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rabbitMqConnection))
        {
            factory.Uri = new Uri(rabbitMqConnection);
        }

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Declare exchanges and queues
        _channel.ExchangeDeclare("system.events", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("metrics.queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("alerts.queue", durable: true, exclusive: false, autoDelete: false);

        _channel.QueueBind("metrics.queue", "system.events", "metrics.*");
        _channel.QueueBind("alerts.queue", "system.events", "alerts.*");

        // Set up consumer
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            _logger.LogInformation("RabbitMQ message received - Key: {Key}, Message: {Message}", routingKey, message);

            // Log event to MongoDB
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IMongoEventService>();

            var eventLog = new EventLog
            {
                EventType = "RABBITMQ_MESSAGE",
                Message = message,
                Source = routingKey,
                Metadata = new Dictionary<string, object>
                {
                    ["routingKey"] = routingKey,
                    ["exchange"] = ea.Exchange,
                    ["consumerTag"] = ea.ConsumerTag
                }
            };

            await eventService.InsertEventAsync(eventLog);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume("metrics.queue", false, consumer);
        _channel.BasicConsume("alerts.queue", false, consumer);

        _logger.LogInformation("RabbitMQ Service started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        await Task.CompletedTask;
    }

    public async Task PublishAsync(string exchange, string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(exchange, routingKey, properties, body);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}