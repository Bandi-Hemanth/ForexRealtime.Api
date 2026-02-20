using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ForexRealtime.Api.Features.SubscriptionAudit;

namespace ForexRealtime.Api.Hubs;

[Authorize]
public class ForexHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<ForexHub> _logger;

    public ForexHub(IMediator mediator, ILogger<ForexHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
        _logger.LogInformation("Hub connection: ConnectionId={ConnectionId}, UserId={UserId}", Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Hub disconnect: ConnectionId={ConnectionId}, Error={Error}", Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Subscribe(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return;
        var normalized = symbol.Trim().ToUpperInvariant();
        var groupName = "symbol_" + normalized.Replace(":", "_", StringComparison.Ordinal);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
        await _mediator.Send(new RecordSubscriptionCommand(userId, normalized, "Subscribe"));
        _logger.LogInformation("Subscribe: UserId={UserId}, Symbol={Symbol}, Group={Group}", userId, normalized, groupName);
    }

    public async Task Unsubscribe(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return;
        var normalized = symbol.Trim().ToUpperInvariant();
        var groupName = "symbol_" + normalized.Replace(":", "_", StringComparison.Ordinal);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
        await _mediator.Send(new RecordSubscriptionCommand(userId, normalized, "Unsubscribe"));
        _logger.LogInformation("Unsubscribe: UserId={UserId}, Symbol={Symbol}", userId, normalized);
    }
}
