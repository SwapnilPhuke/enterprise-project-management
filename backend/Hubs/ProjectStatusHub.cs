using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ProjectManagement.Hubs;

/// <summary>
/// SignalR hub for real-time project status updates.
/// Clients subscribe to per-project groups and receive live status changes.
/// </summary>
[Authorize]
public class ProjectStatusHub : Hub
{
    private readonly ILogger<ProjectStatusHub> _logger;

    public ProjectStatusHub(ILogger<ProjectStatusHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds the caller to a project-specific group so they receive
    /// updates only for that project.
    /// </summary>
    public async Task JoinProjectGroup(int projectId)
    {
        var groupName = GetGroupName(projectId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Connection {ConnectionId} joined group {Group}",
            Context.ConnectionId, groupName);
    }

    /// <summary>Removes the caller from a project-specific group.</summary>
    public async Task LeaveProjectGroup(int projectId)
    {
        var groupName = GetGroupName(projectId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Connection {ConnectionId} left group {Group}",
            Context.ConnectionId, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public static string GetGroupName(int projectId) => $"project-{projectId}";
}
