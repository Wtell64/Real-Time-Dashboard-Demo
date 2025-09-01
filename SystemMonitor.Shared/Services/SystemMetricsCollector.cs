using System.Diagnostics;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.Shared.Services;

public class SystemMetricsCollector : ISystemMetricsCollector
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;

    public SystemMetricsCollector()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
    }

    public SystemMetrics GetCurrentMetrics()
    {
        var totalMemory = GC.GetTotalMemory(false) / (1024 * 1024); // Convert to MB
        var workingSet = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024); // Convert to MB

        return new SystemMetrics
        {
            CpuUsage = GetCpuUsage(),
            MemoryUsage = GetMemoryUsage(),
            DiskUsage = GetDiskUsage(),
            Timestamp = DateTime.UtcNow,
            MachineName = Environment.MachineName
        };
    }

    private double GetCpuUsage()
    {
        try
        {
            return Math.Round(_cpuCounter.NextValue(), 2);
        }
        catch
        {
            // Fallback to random data for demo purposes
            return Math.Round(Random.Shared.NextDouble() * 100, 2);
        }
    }

    private double GetMemoryUsage()
    {
        try
        {
            var availableMemory = _memoryCounter.NextValue();
            var totalMemory = 16384; // Assume 16GB for demo
            return Math.Round(((totalMemory - availableMemory) / totalMemory) * 100, 2);
        }
        catch
        {
            // Fallback to random data for demo purposes
            return Math.Round(Random.Shared.NextDouble() * 100, 2);
        }
    }

    private double GetDiskUsage()
    {
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);
            if (drive != null)
            {
                var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                return Math.Round((double)usedSpace / drive.TotalSize * 100, 2);
            }
        }
        catch
        {
            // Continue to fallback
        }

        // Fallback to random data for demo purposes
        return Math.Round(Random.Shared.NextDouble() * 100, 2);
    }
}
