﻿namespace SystemMonitor.EventService.Services;

public interface IMqttService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task PublishAsync(string topic, string payload);
}