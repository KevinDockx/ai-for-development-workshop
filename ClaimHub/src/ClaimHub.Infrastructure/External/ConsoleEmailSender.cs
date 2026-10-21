using Microsoft.Extensions.Logging;

namespace ClaimHub.Infrastructure.External;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation("Email to {Recipient}: {Subject}", message.To, message.Subject);
        Console.WriteLine($"--- email to {message.To} ---");
        Console.WriteLine(message.Subject);
        Console.WriteLine(message.Body);

        return Task.CompletedTask;
    }
}
