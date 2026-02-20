using System.Collections.Concurrent;

namespace ForexRealtime.Api.Services;

public class TickCache : ITickCache
{
    private readonly ConcurrentDictionary<string, TickSnapshot> _cache = new();

    public void Set(string symbol, TickSnapshot snapshot) => _cache[symbol] = snapshot;
    public TickSnapshot? Get(string symbol) => _cache.TryGetValue(symbol, out var v) ? v : null;
    public IReadOnlyDictionary<string, TickSnapshot> GetAll() => _cache;
}
