using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ForexRealtime.Api.Domain;
using ForexRealtime.Api.Infrastructure.Persistence;

namespace ForexRealtime.Api.Services;

public class TickPersistenceService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITickCache _cache;
    private readonly ILogger<TickPersistenceService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    public TickPersistenceService(IServiceScopeFactory scopeFactory, ITickCache cache, ILogger<TickPersistenceService> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
                var all = _cache.GetAll();
                if (all.Count == 0) continue;

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ForexDbContext>();
                foreach (var (_, snapshot) in all)
                {
                    db.PriceTicks.Add(new PriceTick
                    {
                        Symbol = snapshot.Symbol,
                        Price = snapshot.Price,
                        Bid = snapshot.Bid,
                        Ask = snapshot.Ask,
                        Ts = snapshot.Ts
                    });
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tick persistence error");
            }
        }
    }
}
