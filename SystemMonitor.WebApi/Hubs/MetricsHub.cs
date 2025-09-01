using Microsoft.AspNetCore.SignalR;

namespace SystemMonitor.WebApi.Hubs;

public class MetricsHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "MetricsGroup");
        await base.OnConnectedAsync();
    }
}
