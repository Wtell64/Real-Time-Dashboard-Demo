using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SystemMonitor.AvaloniaUI.Models;

public class MetricsViewModel : INotifyPropertyChanged
{
    private double _cpuUsage;
    private double _memoryUsage;
    private double _diskUsage;
    private string _machineName = Environment.MachineName;
    private DateTime _lastUpdate = DateTime.Now;
    private string _connectionStatus = "Disconnected";

    public double CpuUsage
    {
        get => _cpuUsage;
        set => SetProperty(ref _cpuUsage, value);
    }

    public double MemoryUsage
    {
        get => _memoryUsage;
        set => SetProperty(ref _memoryUsage, value);
    }

    public double DiskUsage
    {
        get => _diskUsage;
        set => SetProperty(ref _diskUsage, value);
    }

    public string MachineName
    {
        get => _machineName;
        set => SetProperty(ref _machineName, value);
    }

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set => SetProperty(ref _lastUpdate, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public ObservableCollection<ChartDataPoint> CpuHistory { get; } = new();
    public ObservableCollection<ChartDataPoint> MemoryHistory { get; } = new();
    public ObservableCollection<ChartDataPoint> DiskHistory { get; } = new();
    public ObservableCollection<EventLogItem> RecentEvents { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public class ChartDataPoint
{
    public DateTime Time { get; set; }
    public double Value { get; set; }
}

public class EventLogItem
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = "";
    public string Message { get; set; } = "";
    public string Source { get; set; } = "";
}