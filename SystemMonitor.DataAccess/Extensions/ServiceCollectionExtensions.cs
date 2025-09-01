using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;
using SystemMonitor.DataAccess.Context;
using SystemMonitor.DataAccess.Services;

namespace SystemMonitor.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services,
        string postgresConnectionString,
        string redisConnectionString,
        string mongoConnectionString)
    {
        // PostgreSQL
        services.AddDbContext<SystemMonitorContext>(options =>
            options.UseNpgsql(postgresConnectionString));

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // MongoDB
        services.AddSingleton<IMongoClient>(sp =>
            new MongoClient(mongoConnectionString));

        // Services
        services.AddScoped<IMetricsRepository, MetricsRepository>();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IMongoEventService, MongoEventService>();

        return services;
    }
}