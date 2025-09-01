using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemMonitor.DataAccess.Extensions;
using SystemMonitor.EventService.Services;
using SystemMonitor.Shared.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddSharedServices();

builder.Services.AddDataAccessServices(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=SystemMonitor;Username=admin;Password=admin123",
    builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379,password=redis123",
    builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:admin123@localhost:27017/SystemMonitor?authSource=admin"
);

// Event services
builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddHostedService<EventPublisherService>();

var host = builder.Build();

// Start MQTT and RabbitMQ services
var mqttService = host.Services.GetRequiredService<IMqttService>();
var rabbitMqService = host.Services.GetRequiredService<IRabbitMqService>();

await mqttService.StartAsync(CancellationToken.None);
await rabbitMqService.StartAsync(CancellationToken.None);

await host.RunAsync();