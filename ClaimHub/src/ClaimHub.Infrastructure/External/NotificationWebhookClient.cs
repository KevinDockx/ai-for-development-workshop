using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ClaimHub.Infrastructure.External;

public interface INotificationWebhookClient
{
    Task<bool> NotifyAsync(string eventName, object payload, CancellationToken cancellationToken = default);
}

public class NotificationWebhookClient : INotificationWebhookClient
{
    private readonly NotificationWebhookOptions _options;

    public NotificationWebhookClient(IOptions<NotificationWebhookOptions> options)
    {
        _options = options.Value;
    }

    public async Task<bool> NotifyAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.Url))
        {
            return false;
        }

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds) };
        string json = JsonSerializer.Serialize(new { @event = eventName, payload });
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(_options.Url, content, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
