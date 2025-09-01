using Microsoft.EntityFrameworkCore;
using SystemMonitor.WebApi.Hubs;
using SystemMonitor.WebApi.Services;
using SystemMonitor.Shared.Extensions;
using SystemMonitor.DataAccess.Extensions;
using SystemMonitor.DataAccess.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add custom services
builder.Services.AddSharedServices();

var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
builder.Services.AddDataAccessServices(
    connectionStrings.GetConnectionString("DefaultConnection")!,
    connectionStrings.GetConnectionString("Redis")!,
    connectionStrings.GetConnectionString("MongoDB")!
);

// Background service
builder.Services.AddHostedService<BackgroundMetricsService>();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();

app.MapControllers();
app.MapHub<MetricsHub>("/metrics-hub");
app.MapHealthChecks("/health");

app.Run();