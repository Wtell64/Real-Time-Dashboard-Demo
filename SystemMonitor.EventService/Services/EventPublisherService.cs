using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using SystemMonitor.Shared.Services;

namespace SystemMonitor.EventService.Services;

public class EventPublisherService : BackgroundService
{
    private readonly ISystemMetricsCollector _metricsCollector;
    private readonly IMqttService _mqttService;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ILogger<EventPublisherService> _logger;

    public EventPublisherService(
        ISystemMetricsCollector metricsCollector,
        IMqttService mqttService,
        IRabbitMqService rabbitMqService,
        ILogger<EventPublisherService> logger)
    {
        _metricsCollector = metricsCollector;
        _mqttService = mqttService;
        _rabbitMqService = rabbitMqService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var metrics = _metricsCollector.GetCurrentMetrics();
                var json = JsonSerializer.Serialize(metrics);

                // Publish to MQTT
                await _mqttService.PublishAsync($"system/{metrics.MachineName}/metrics", json);

                // Publish to RabbitMQ
                await _rabbitMqService.PublishAsync("system.events", "metrics.collected", json);

                // Check for alerts
                if (metrics.CpuUsage > 80 || metrics.MemoryUsage > 85 || metrics.DiskUsage > 90)
                {
                    var alert = new
                    {
                        Type = "HIGH_USAGE_ALERT",
                        Machine = metrics.MachineName,
                        Timestamp = metrics.Timestamp,
                        Metrics = new { metrics.CpuUsage, metrics.MemoryUsage, metrics.DiskUsage },
                        Message = $"High resource usage detected: CPU {metrics.CpuUsage}%, Memory {metrics.MemoryUsage}%, Disk {metrics.DiskUsage}%"
                    };

                    var alertJson = JsonSerializer.Serialize(alert);

                    await _mqttService.PublishAsync($"system/{metrics.MachineName}/alerts", alertJson);
                    await _rabbitMqService.PublishAsync("system.events", "alerts.high_usage", alertJson);
                }

                _logger.LogInformation("Events published - CPU: {Cpu}%, Memory: {Memory}%, Disk: {Disk}%",
                    metrics.CpuUsage, metrics.MemoryUsage, metrics.DiskUsage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event publisher service");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}