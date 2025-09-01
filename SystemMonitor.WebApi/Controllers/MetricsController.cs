using Microsoft.AspNetCore.Mvc;
using SystemMonitor.Shared.Models;
using SystemMonitor.Shared.Services;
using SystemMonitor.DataAccess.Services;

namespace SystemMonitor.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly ISystemMetricsCollector _metricsCollector;
    private readonly IMetricsRepository _repository;
    private readonly IRedisCacheService _cache;

    public MetricsController(
        ISystemMetricsCollector metricsCollector,
        IMetricsRepository repository,
        IRedisCacheService cache)
    {
        _metricsCollector = metricsCollector;
        _repository = repository;
        _cache = cache;
    }

    [HttpGet("current")]
    public async Task<ActionResult<SystemMetrics>> GetCurrentMetrics()
    {
        // Try cache first
        var cached = await _cache.GetAsync<SystemMetrics>("current_metrics");
        if (cached != null && (DateTime.UtcNow - cached.Timestamp).TotalSeconds < 30)
        {
            return Ok(cached);
        }

        // Generate new metrics
        var metrics = _metricsCollector.GetCurrentMetrics();
        await _repository.AddAsync(metrics);

        // Cache for 30 seconds
        await _cache.SetAsync("current_metrics", metrics, TimeSpan.FromSeconds(30));

        return Ok(metrics);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<List<SystemMetrics>>> GetRecentMetrics([FromQuery] int count = 50)
    {
        var metrics = await _repository.GetRecentAsync(count);
        return Ok(metrics);
    }

    [HttpGet("range")]
    public async Task<ActionResult<List<SystemMetrics>>> GetMetricsByRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var metrics = await _repository.GetByDateRangeAsync(from, to);
        return Ok(metrics);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<SystemMetrics>> GetLatestMetrics()
    {
        var metrics = await _repository.GetLatestAsync();
        if (metrics == null)
            return NotFound();

        return Ok(metrics);
    }
}