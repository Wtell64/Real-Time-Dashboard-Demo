using Grpc.Net.Client;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SystemMonitor.Shared.Grpc;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.AvaloniaUI.Services;

public class DataService : IDataService, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly GrpcChannel _grpcChannel;
    private readonly SystemMetricsService.SystemMetricsServiceClient _grpcClient;
    private HubConnection? _hubConnection;

    public event Action<SystemMetrics>? MetricsUpdated;

    public DataService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };

        _grpcChannel = GrpcChannel.ForAddress("http://localhost:5001");
        _grpcClient = new SystemMetricsService.SystemMetricsServiceClient(_grpcChannel);
    }

    public async Task<SystemMetrics?> GetCurrentMetricsAsync()
    {
        try
        {
            // Try REST API first
            var response = await _httpClient.GetFromJsonAsync<SystemMetrics>("api/metrics/current");
            return response;
        }
        catch
        {
            try
            {
                // Fallback to gRPC
                var grpcResponse = await _grpcClient.GetCurrentMetricsAsync(new Empty());
                return new SystemMetrics
                {
                    Id = grpcResponse.Id,
                    Timestamp = new DateTime(grpcResponse.Timestamp),
                    CpuUsage = grpcResponse.CpuUsage,
                    MemoryUsage = grpcResponse.MemoryUsage,
                    DiskUsage = grpcResponse.DiskUsage,
                    MachineName = grpcResponse.MachineName
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public async Task<List<SystemMetrics>> GetRecentMetricsAsync(int count = 50)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<SystemMetrics>>($"api/metrics/recent?count={count}");
            return response ?? new List<SystemMetrics>();
        }
        catch
        {
            return new List<SystemMetrics>();
        }
    }

    public async Task<List<EventLog>> GetRecentEventsAsync(int count = 50)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<EventLog>>($"api/events/recent?count={count}");
            return response ?? new List<EventLog>();
        }
        catch
        {
            return new List<EventLog>();
        }
    }

    public async Task StartRealtimeConnectionAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/metrics-hub")
            .Build();

        _hubConnection.On<SystemMetrics>("MetricsUpdate", (metrics) =>
        {
            MetricsUpdated?.Invoke(metrics);
        });

        await _hubConnection.StartAsync();
    }

    public async Task StopRealtimeConnectionAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopRealtimeConnectionAsync();
        _httpClient.Dispose();
        await _grpcChannel.ShutdownAsync();
        _grpcChannel.Dispose();
    }
}