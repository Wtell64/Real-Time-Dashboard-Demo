using Microsoft.Extensions.DependencyInjection;
using SystemMonitor.Shared.Services;

namespace SystemMonitor.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<ISystemMetricsCollector, SystemMetricsCollector>();
        return services;
    }
}