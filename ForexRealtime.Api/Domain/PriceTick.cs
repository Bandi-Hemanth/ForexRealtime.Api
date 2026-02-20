namespace ForexRealtime.Api.Domain;

public class PriceTick
{
    public long Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public DateTime Ts { get; set; }
}
