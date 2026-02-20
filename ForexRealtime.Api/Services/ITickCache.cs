namespace ForexRealtime.Api.Services;

public interface ITickCache
{
    void Set(string symbol, TickSnapshot snapshot);
    TickSnapshot? Get(string symbol);
    IReadOnlyDictionary<string, TickSnapshot> GetAll();
}

public record TickSnapshot(string Symbol, decimal Price, decimal Bid, decimal Ask, DateTime Ts);
