using Microsoft.AspNetCore.Mvc;
using SystemMonitor.Shared.Models;
using SystemMonitor.DataAccess.Services;

namespace SystemMonitor.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMongoEventService _eventService;

    public EventsController(IMongoEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("recent")]
    public async Task<ActionResult<List<EventLog>>> GetRecentEvents([FromQuery] int count = 50)
    {
        var events = await _eventService.GetRecentEventsAsync(count);
        return Ok(events);
    }

    [HttpGet("by-type/{eventType}")]
    public async Task<ActionResult<List<EventLog>>> GetEventsByType(string eventType, [FromQuery] int count = 50)
    {
        var events = await _eventService.GetEventsByTypeAsync(eventType, count);
        return Ok(events);
    }

    [HttpPost]
    public async Task<ActionResult<EventLog>> CreateEvent([FromBody] EventLog eventLog)
    {
        await _eventService.InsertEventAsync(eventLog);
        return CreatedAtAction(nameof(GetRecentEvents), eventLog);
    }
}
