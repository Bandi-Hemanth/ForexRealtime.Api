namespace ForexRealtime.Api.Domain;

public class SubscriptionAudit
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "Subscribe" | "Unsubscribe"
    public DateTime At { get; set; }
}
