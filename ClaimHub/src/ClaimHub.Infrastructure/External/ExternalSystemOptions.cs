namespace ClaimHub.Infrastructure.External;

public class PaymentGatewayOptions
{
    public int MinLatencyMs { get; set; } = 25;

    public int MaxLatencyMs { get; set; } = 150;

    public decimal MaxAmount { get; set; } = 250000m;
}

public class NotificationWebhookOptions
{
    public bool Enabled { get; set; }

    public string Url { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 10;
}
