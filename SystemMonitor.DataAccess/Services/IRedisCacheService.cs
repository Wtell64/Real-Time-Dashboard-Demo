namespace SystemMonitor.DataAccess.Services;

public interface IRedisCacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task DeleteAsync(string key);
    Task PublishAsync(string channel, string message);
    Task SubscribeAsync(string channel, Action<string> handler);
}