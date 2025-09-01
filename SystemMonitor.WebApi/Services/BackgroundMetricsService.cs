using Microsoft.AspNetCore.SignalR;
using SystemMonitor.WebApi.Hubs;
using SystemMonitor.Shared.Services;
using SystemMonitor.DataAccess.Services;

namespace SystemMonitor.WebApi.Services;

public class BackgroundMetricsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<MetricsHub> _hubContext;
    private readonly ILogger<BackgroundMetricsService> _logger;

    public BackgroundMetricsService(
        IServiceProvider serviceProvider,
        IHubContext<MetricsHub> hubContext,
        ILogger<BackgroundMetricsService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metricsCollector = scope.ServiceProvider.GetRequiredService<ISystemMetricsCollector>();
                var repository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();
                var cache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();

                var metrics = metricsCollector.GetCurrentMetrics();
                await repository.AddAsync(metrics);

                // Update cache
                await cache.SetAsync("current_metrics", metrics, TimeSpan.FromMinutes(5));

                // Publish to Redis
                await cache.PublishAsync("metrics_updates", System.Text.Json.JsonSerializer.Serialize(metrics));

                // Send to WebSocket clients
                await _hubContext.Clients.Group("MetricsGroup").SendAsync("MetricsUpdate", metrics, stoppingToken);

                _logger.LogInformation("Metrics updated and broadcasted: CPU {Cpu}%, Memory {Memory}%, Disk {Disk}%",
                    metrics.CpuUsage, metrics.MemoryUsage, metrics.DiskUsage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background metrics service");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}