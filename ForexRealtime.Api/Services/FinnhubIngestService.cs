using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForexRealtime.Api.Services;

public class FinnhubIngestService : BackgroundService
{
    private readonly ITickCache _cache;
    private readonly ILogger<FinnhubIngestService> _logger;
    private readonly string _apiKey;
    private readonly string[] _symbols;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public FinnhubIngestService(ITickCache cache, ILogger<FinnhubIngestService> logger, IConfiguration config)
    {
        _cache = cache;
        _logger = logger;
        _apiKey = config["Finnhub:ApiKey"] ?? "d1ehg3pr01qjssrk5gkgd1ehg3pr01qjssrk5g10";
        _symbols = (config["Finnhub:Symbols"] ?? "OANDA:EUR_USD,OANDA:GBP_USD,OANDA:USD_JPY,BINANCE:BTCUSDT")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunWebSocketAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Finnhub WebSocket error; reconnecting in 10s");
            }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task RunWebSocketAsync(CancellationToken ct)
    {
        var uri = new Uri($"wss://ws.finnhub.io?token={_apiKey}");
        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(uri, ct);
        _logger.LogInformation("Finnhub WebSocket connected");

        foreach (var symbol in _symbols)
        {
            var sub = JsonSerializer.Serialize(new { type = "subscribe", symbol });
            await ws.SendAsync(Encoding.UTF8.GetBytes(sub), WebSocketMessageType.Text, true, ct);
            _logger.LogInformation("Subscribed to {Symbol}", symbol);
        }

        var buffer = new byte[4096];
        var segment = new ArraySegment<byte>(buffer);

        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await ws.ReceiveAsync(segment, ct);
            if (result.MessageType == WebSocketMessageType.Close) break;
            if (result.MessageType != WebSocketMessageType.Text || !result.EndOfMessage) continue;

            var json = Encoding.UTF8.GetString(buffer.AsSpan(0, result.Count));
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("type", out var typeEl))
                {
                    var type = typeEl.GetString();
                    if (type == "trade" && doc.RootElement.TryGetProperty("data", out var data))
                    {
                        foreach (var item in data.EnumerateArray())
                        {
                            ParseTrade(item, doc.RootElement.TryGetProperty("symbol", out var symEl) ? symEl.GetString() : null);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Finnhub message parse skip: {Json}", json.Length > 200 ? json[..200] + "..." : json);
            }
        }
    }

    private void ParseTrade(JsonElement item, string? symbol)
    {
        try
        {
            var p = item.TryGetProperty("p", out var pEl) ? pEl.GetDecimal() : 0m;
            string sym;

            if (!string.IsNullOrEmpty(symbol))
            {
                sym = symbol;
            }
            else if (item.TryGetProperty("s", out var sEl))
            {
                sym = sEl.GetString() ?? "UNKNOWN";
            }
            else
            {
                sym = "UNKNOWN";
            }
            if (string.IsNullOrEmpty(sym)) return;
            var ts = item.TryGetProperty("t", out var tEl) ? DateTimeOffset.FromUnixTimeMilliseconds(tEl.GetInt64()).UtcDateTime : DateTime.UtcNow;
            var spread = 0.0001m;
            var snapshot = new TickSnapshot(sym, p, p - spread, p + spread, ts);
            _cache.Set(sym, snapshot);
        }
        catch { /* ignore single parse errors */ }
    }
}
