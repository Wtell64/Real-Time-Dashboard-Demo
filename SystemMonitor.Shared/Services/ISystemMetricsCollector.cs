

using SystemMonitor.Shared.Models;

namespace SystemMonitor.Shared.Services
{
    public interface ISystemMetricsCollector
    {
        SystemMetrics GetCurrentMetrics();
    }
}
