using Grpc.Core;
using SystemMonitor.Shared.Grpc;
using SystemMonitor.Shared.Services;
using SystemMonitor.DataAccess.Services;

namespace SystemMonitor.GrpcService.Services;

public class SystemMetricsGrpcService : SystemMetricsService.SystemMetricsServiceBase //Needs proto project to be build first
{
    private readonly ISystemMetricsCollector _metricsCollector;
    private readonly IMetricsRepository _repository;
    private readonly ILogger<SystemMetricsGrpcService> _logger;

    public SystemMetricsGrpcService(
        ISystemMetricsCollector metricsCollector,
        IMetricsRepository repository,
        ILogger<SystemMetricsGrpcService> logger)
    {
        _metricsCollector = metricsCollector;
        _repository = repository;
        _logger = logger;
    }

    public override async Task<MetricsReply> GetCurrentMetrics(Empty request, ServerCallContext context)
    {
        var metrics = _metricsCollector.GetCurrentMetrics();

        // Save to database
        await _repository.AddAsync(metrics);

        _logger.LogInformation("Generated metrics: CPU {Cpu}%, Memory {Memory}%, Disk {Disk}%",
            metrics.CpuUsage, metrics.MemoryUsage, metrics.DiskUsage);

        return new MetricsReply
        {
            Id = metrics.Id,
            Timestamp = metrics.Timestamp.Ticks,
            CpuUsage = metrics.CpuUsage,
            MemoryUsage = metrics.MemoryUsage,
            DiskUsage = metrics.DiskUsage,
            MachineName = metrics.MachineName
        };
    }

    public override async Task GetMetricsStream(Empty request, IServerStreamWriter<MetricsReply> responseStream, ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var metrics = _metricsCollector.GetCurrentMetrics();
            await _repository.AddAsync(metrics);

            var reply = new MetricsReply
            {
                Id = metrics.Id,
                Timestamp = metrics.Timestamp.Ticks,
                CpuUsage = metrics.CpuUsage,
                MemoryUsage = metrics.MemoryUsage,
                DiskUsage = metrics.DiskUsage,
                MachineName = metrics.MachineName
            };

            await responseStream.WriteAsync(reply);
            await Task.Delay(TimeSpan.FromMinutes(2), context.CancellationToken);
        }
    }

    public override async Task<HistoricalReply> GetHistoricalMetrics(HistoricalRequest request, ServerCallContext context)
    {
        var fromDate = new DateTime(request.FromTimestamp);
        var toDate = new DateTime(request.ToTimestamp);

        var metrics = await _repository.GetByDateRangeAsync(fromDate, toDate);

        var reply = new HistoricalReply();
        reply.Metrics.AddRange(metrics.Take(request.Limit).Select(m => new MetricsReply
        {
            Id = m.Id,
            Timestamp = m.Timestamp.Ticks,
            CpuUsage = m.CpuUsage,
            MemoryUsage = m.MemoryUsage,
            DiskUsage = m.DiskUsage,
            MachineName = m.MachineName
        }));

        return reply;
    }
}