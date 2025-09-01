using SystemMonitor.Shared.Models;

namespace SystemMonitor.DataAccess.Services;

public interface IMetricsRepository
{
    Task<SystemMetrics> AddAsync(SystemMetrics metrics);
    Task<List<SystemMetrics>> GetRecentAsync(int count = 100);
    Task<List<SystemMetrics>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<SystemMetrics?> GetLatestAsync();
}