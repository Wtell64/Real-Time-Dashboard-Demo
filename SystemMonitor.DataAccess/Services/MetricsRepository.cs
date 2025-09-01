using Microsoft.EntityFrameworkCore;
using SystemMonitor.DataAccess.Context;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.DataAccess.Services;

public class MetricsRepository : IMetricsRepository
{
    private readonly SystemMonitorContext _context;

    public MetricsRepository(SystemMonitorContext context)
    {
        _context = context;
    }

    public async Task<SystemMetrics> AddAsync(SystemMetrics metrics)
    {
        _context.SystemMetrics.Add(metrics);
        await _context.SaveChangesAsync();
        return metrics;
    }

    public async Task<List<SystemMetrics>> GetRecentAsync(int count = 100)
    {
        return await _context.SystemMetrics
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<SystemMetrics>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.SystemMetrics
            .Where(m => m.Timestamp >= from && m.Timestamp <= to)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<SystemMetrics?> GetLatestAsync()
    {
        return await _context.SystemMetrics
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();
    }
}