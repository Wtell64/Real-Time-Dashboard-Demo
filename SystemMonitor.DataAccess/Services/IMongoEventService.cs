using SystemMonitor.Shared.Models;

namespace SystemMonitor.DataAccess.Services;

public interface IMongoEventService
{
    Task InsertEventAsync(EventLog eventLog);
    Task<List<EventLog>> GetRecentEventsAsync(int count = 50);
    Task<List<EventLog>> GetEventsByTypeAsync(string eventType, int count = 50);
}