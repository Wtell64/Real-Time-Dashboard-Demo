using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.AvaloniaUI.Services;

public interface IDataService
{
    Task<SystemMetrics?> GetCurrentMetricsAsync();
    Task<List<SystemMetrics>> GetRecentMetricsAsync(int count = 50);
    Task<List<EventLog>> GetRecentEventsAsync(int count = 50);
    Task StartRealtimeConnectionAsync();
    Task StopRealtimeConnectionAsync();
    event Action<SystemMetrics>? MetricsUpdated;
}