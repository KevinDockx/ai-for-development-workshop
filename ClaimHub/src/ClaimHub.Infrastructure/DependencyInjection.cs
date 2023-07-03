using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.External;
using ClaimHub.Infrastructure.Mediator;
using ClaimHub.Infrastructure.Repositories;
using ClaimHub.Infrastructure.Reporting;
using ClaimHub.Infrastructure.Services;
using ClaimHub.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimHub.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "ClaimHub";

    /// <param name="configureDbContext">
    /// Optional hook for hosts that need extra context options, for example lazy-loading proxies in the Admin portal.
    /// </param>
    public static IServiceCollection AddClaimHubInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> configureDbContext = null)
    {
        string connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");
        connectionString = DatabasePaths.ResolveConnectionString(connectionString);

        services.AddDbContext<ClaimHubDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            configureDbContext?.Invoke(options);
        });

        services.Configure<PaymentGatewayOptions>(configuration.GetSection("PaymentGateway"));
        services.Configure<NotificationWebhookOptions>(configuration.GetSection("NotificationWebhook"));
        services.Configure<DocumentStorageOptions>(configuration.GetSection("DocumentStorage"));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        services.AddMiniMediator(typeof(DependencyInjection).Assembly);

        services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
        services.AddSingleton<IAddressLookup, FakeAddressLookup>();
        services.AddSingleton<IIbanCheckService, FakeIbanCheckService>();
        services.AddSingleton<IEmailSender, ConsoleEmailSender>();
        services.AddSingleton<INotificationWebhookClient, NotificationWebhookClient>();
        services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();

        services.AddSingleton<IPolicyNumberSequence, DatabasePolicyNumberSequence>();
        services.AddSingleton<IClaimNumberSequence, DatabaseClaimNumberSequence>();
        services.AddSingleton<IInvoiceNumberSequence, DatabaseInvoiceNumberSequence>();

        services.AddScoped<ClaimHubDatabaseInitializer>();

        services.AddSingleton<PolicyPortfolioService>();
        services.AddSingleton<ClaimDecisionService>();
        services.AddSingleton<DunningScheduleService>();
        services.AddSingleton<PremiumExplanationService>();
        services.AddSingleton<PolicyChangeDecisionService>();
        services.AddSingleton<PolicyHolderOverviewService>();
        services.AddSingleton<AccountStatementService>();
        services.AddSingleton<RenewalOutlookService>();
        services.AddSingleton<InvoiceAgingService>();
        services.AddSingleton<HandlerWorkloadService>();
        services.AddScoped<ReceivablesQuery>();
        services.AddScoped<HandlerWorkloadQuery>();
        services.AddScoped<RenewalPipelineQuery>();
        services.AddScoped<PolicyHolderExportQuery>();
        services.AddScoped<PortfolioReadService>();
        services.AddScoped<AuditTrailQuery>();

        return services;
    }
}
