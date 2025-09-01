using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Text;
using SystemMonitor.DataAccess.Services;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.EventService.Services;

public class MqttService : IMqttService
{
    private readonly IManagedMqttClient _mqttClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MqttService> _logger;
    private readonly string _host;
    private readonly int _port;

    public MqttService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<MqttService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _host = configuration["MqttSettings:Host"] ?? "localhost";
        _port = int.Parse(configuration["MqttSettings:Port"] ?? "1883");

        var factory = new MqttFactory();
        _mqttClient = factory.CreateManagedMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
        _mqttClient.ConnectedAsync += OnConnected;
        _mqttClient.DisconnectedAsync += OnDisconnected;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_host, _port)
            .WithClientId($"SystemMonitor-{Environment.MachineName}-{Guid.NewGuid()}")
            .WithCleanSession()
            .Build();

        var managedOptions = new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(clientOptions)
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .Build();

        await _mqttClient.StartAsync(managedOptions);

        // Subscribe to topics
        await _mqttClient.SubscribeAsync("system/+/metrics");
        await _mqttClient.SubscribeAsync("system/+/alerts");
        await _mqttClient.SubscribeAsync("system/+/events");

        _logger.LogInformation("MQTT Service started and subscribed to topics");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.StopAsync();
        _mqttClient.Dispose();
    }

    public async Task PublishAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(false)
            .Build();

        await _mqttClient.EnqueueAsync(message);
    }

    private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>());

        _logger.LogInformation("MQTT message received - Topic: {Topic}, Payload: {Payload}", topic, payload);

        // Log event to MongoDB
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IMongoEventService>();

        var eventLog = new EventLog
        {
            EventType = "MQTT_MESSAGE",
            Message = payload,
            Source = topic,
            Metadata = new Dictionary<string, object>
            {
                ["topic"] = topic,
                ["qos"] = e.ApplicationMessage.QualityOfServiceLevel.ToString(),
                ["retain"] = e.ApplicationMessage.Retain
            }
        };

        await eventService.InsertEventAsync(eventLog);
    }

    private Task OnConnected(MqttClientConnectedEventArgs e)
    {
        _logger.LogInformation("MQTT Client connected");
        return Task.CompletedTask;
    }

    private Task OnDisconnected(MqttClientDisconnectedEventArgs e)
    {
        _logger.LogWarning("MQTT Client disconnected: {Reason}", e.Reason);
        return Task.CompletedTask;
    }
}