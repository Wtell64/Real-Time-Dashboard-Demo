using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemMonitor.DataAccess.Context;

namespace SystemMonitor.DataAccess.Extensions;

public static class DatabaseExtensions
{
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SystemMonitorContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SystemMonitorContext>>();

        try
        {
            logger.LogInformation("Ensuring database is created and up to date...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database migration");
            throw;
        }
    }

    public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SystemMonitorContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SystemMonitorContext>>();

        try
        {
            // Check if we need to seed data
            if (await context.SystemMetrics.AnyAsync())
            {
                logger.LogInformation("Database already contains data, skipping seed");
                return;
            }

            logger.LogInformation("Seeding initial data...");

            // Add some sample historical data
            var random = new Random();
            var startTime = DateTime.UtcNow.AddDays(-1);

            for (int i = 0; i < 100; i++)
            {
                var timestamp = startTime.AddMinutes(i * 15); // Every 15 minutes
                context.SystemMetrics.Add(new Shared.Models.SystemMetrics
                {
                    Timestamp = timestamp,
                    CpuUsage = Math.Round(random.NextDouble() * 100, 2),
                    MemoryUsage = Math.Round(random.NextDouble() * 100, 2),
                    DiskUsage = Math.Round(50 + random.NextDouble() * 40, 2), // 50-90% range
                    MachineName = Environment.MachineName
                }); 
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }
}