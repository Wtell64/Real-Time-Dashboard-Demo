using SystemMonitor.GrpcService.Services;
using SystemMonitor.Shared.Extensions;
using SystemMonitor.DataAccess.Extensions;
using Microsoft.EntityFrameworkCore;
using SystemMonitor.DataAccess.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddGrpc();
builder.Services.AddSharedServices();

// Add database services
var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
builder.Services.AddDataAccessServices(
    connectionStrings.GetConnectionString("DefaultConnection")!,
    connectionStrings.GetConnectionString("Redis")!,
    connectionStrings.GetConnectionString("MongoDB") ?? "mongodb://admin:admin123@localhost:27017/SystemMonitor?authSource=admin"
);

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionStrings.GetConnectionString("DefaultConnection")!)
    .AddRedis(connectionStrings.GetConnectionString("Redis")!);

var app = builder.Build();

// Migrate database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SystemMonitorContext>();
    await context.Database.MigrateAsync();
}

// Configure pipeline
app.MapGrpcService<SystemMetricsGrpcService>();
app.MapHealthChecks("/health");

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();