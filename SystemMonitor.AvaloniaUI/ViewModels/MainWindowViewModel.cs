using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SystemMonitor.AvaloniaUI.Models;
using SystemMonitor.AvaloniaUI.Services;
using SystemMonitor.Shared.Models;
using Timer = System.Threading.Timer;
namespace SystemMonitor.AvaloniaUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    private readonly Timer _updateTimer;

    public MetricsViewModel Metrics { get; } = new();

    public MainWindowViewModel()
    {
        _dataService = new DataService();

        // Subscribe to real-time updates
        _dataService.MetricsUpdated += OnMetricsUpdated;

        // Start periodic updates and real-time connection
        _ = Task.Run(async () =>
        {
            await InitializeAsync();
            await _dataService.StartRealtimeConnectionAsync();
        });

        // Fallback timer for updates
        _updateTimer = new Timer(UpdateTimerCallback, null,
            0, // Start immediately (0 milliseconds)
            120000); // Repeat every 2 minutes (120,000 milliseconds)
    }

    private async void UpdateTimerCallback(object? state)
    {
        await UpdateDataAsync();
    }

    private async Task InitializeAsync()
    {
        await UpdateDataAsync();
        await LoadEventsAsync();
    }

    private void OnMetricsUpdated(SystemMetrics metrics)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            UpdateMetricsDisplay(metrics);
        });
    }

    private async Task UpdateDataAsync()
    {
        try
        {
            var current = await _dataService.GetCurrentMetricsAsync();
            if (current != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    UpdateMetricsDisplay(current);
                    Metrics.ConnectionStatus = "Connected";
                });
            }
        }
        catch
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Metrics.ConnectionStatus = "Disconnected";
            });
        }
    }

    private void UpdateMetricsDisplay(SystemMetrics metrics)
    {
        Metrics.CpuUsage = metrics.CpuUsage;
        Metrics.MemoryUsage = metrics.MemoryUsage;
        Metrics.DiskUsage = metrics.DiskUsage;
        Metrics.MachineName = metrics.MachineName;
        Metrics.LastUpdate = DateTime.Now;

        // Add to history (keep last 50 points)
        var now = DateTime.Now;
        Metrics.CpuHistory.Add(new ChartDataPoint { Time = now, Value = metrics.CpuUsage });
        Metrics.MemoryHistory.Add(new ChartDataPoint { Time = now, Value = metrics.MemoryUsage });
        Metrics.DiskHistory.Add(new ChartDataPoint { Time = now, Value = metrics.DiskUsage });

        // Keep only last 50 points
        while (Metrics.CpuHistory.Count > 50) Metrics.CpuHistory.RemoveAt(0);
        while (Metrics.MemoryHistory.Count > 50) Metrics.MemoryHistory.RemoveAt(0);
        while (Metrics.DiskHistory.Count > 50) Metrics.DiskHistory.RemoveAt(0);
    }

    private async Task LoadEventsAsync()
    {
        try
        {
            var events = await _dataService.GetRecentEventsAsync(10);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Metrics.RecentEvents.Clear();
                foreach (var evt in events.Take(10))
                {
                    Metrics.RecentEvents.Add(new EventLogItem
                    {
                        Timestamp = evt.Timestamp,
                        EventType = evt.EventType,
                        Message = evt.Message,
                        Source = evt.Source
                    });
                }
            });
        }
        catch
        {
            // Handle error silently
        }
    }
}