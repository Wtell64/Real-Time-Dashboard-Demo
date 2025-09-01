using MongoDB.Driver;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.DataAccess.Services;

public class MongoEventService : IMongoEventService
{
    private readonly IMongoCollection<EventLog> _eventCollection;

    public MongoEventService(IMongoClient mongoClient, string databaseName = "SystemMonitor")
    {
        var database = mongoClient.GetDatabase(databaseName);
        _eventCollection = database.GetCollection<EventLog>("Events");
    }

    public async Task InsertEventAsync(EventLog eventLog)
    {
        await _eventCollection.InsertOneAsync(eventLog);
    }

    public async Task<List<EventLog>> GetRecentEventsAsync(int count = 50)
    {
        return await _eventCollection
            .Find(_ => true)
            .SortByDescending(e => e.Timestamp)
            .Limit(count)
            .ToListAsync();
    }

    public async Task<List<EventLog>> GetEventsByTypeAsync(string eventType, int count = 50)
    {
        return await _eventCollection
            .Find(e => e.EventType == eventType)
            .SortByDescending(e => e.Timestamp)
            .Limit(count)
            .ToListAsync();
    }
}