using StackExchange.Redis;
using System.Text.Json;

namespace SystemMonitor.DataAccess.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ISubscriber _subscriber;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _subscriber = redis.GetSubscriber();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _database.StringGetAsync(key);
        if (!json.HasValue) return default;

        return JsonSerializer.Deserialize<T>(json!);
    }

    public async Task DeleteAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task PublishAsync(string channel, string message)
    {
        await _subscriber.PublishAsync(channel, message);
    }

    public async Task SubscribeAsync(string channel, Action<string> handler)
    {
        await _subscriber.SubscribeAsync(channel, (ch, message) => handler(message!));
    }
}