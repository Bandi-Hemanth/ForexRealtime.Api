using Microsoft.AspNetCore.SignalR;

namespace ForexRealtime.Api.Services;

public class BroadcastService : BackgroundService
{
    private readonly IHubContext<Hubs.ForexHub> _hubContext;
    private readonly ITickCache _cache;
    private readonly ILogger<BroadcastService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    public BroadcastService(
        IHubContext<Hubs.ForexHub> hubContext,
        ITickCache cache,
        ILogger<BroadcastService> logger)
    {
        _hubContext = hubContext;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var all = _cache.GetAll();
                if (all.Count > 0)
                {
                    foreach (var (symbol, snapshot) in all)
                    {
                        var groupName = "symbol_" + symbol.Replace(":", "_", StringComparison.Ordinal);
                        var payload = new
                        {
                            snapshot.Symbol,
                            snapshot.Price,
                            snapshot.Bid,
                            snapshot.Ask,
                            snapshot.Ts
                        };
                        await _hubContext.Clients.Group(groupName).SendAsync("PriceUpdate", payload, stoppingToken);
                    }
                    _logger.LogDebug("Broadcast sent for {Count} symbols", all.Count);
                }
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Broadcast error");
            }
        }
    }
}
