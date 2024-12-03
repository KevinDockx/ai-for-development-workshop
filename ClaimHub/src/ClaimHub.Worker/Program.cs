using AutoMapper;
using ClaimHub.Api.Policies;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Worker.Jobs;
using ClaimHub.Worker.Options;
using ClaimHub.Worker.Planning;
using ClaimHub.Worker.Scheduling;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddClaimHubInfrastructure(configuration);

// Job settings and clock
services.Configure<JobClockOptions>(configuration.GetSection(JobClockOptions.SectionName));
services.Configure<BillingRunJobOptions>(configuration.GetSection(BillingRunJobOptions.SectionName));
services.Configure<DunningJobOptions>(configuration.GetSection(DunningJobOptions.SectionName));
services.Configure<ClaimSlaEscalationJobOptions>(configuration.GetSection(ClaimSlaEscalationJobOptions.SectionName));
services.Configure<RenewalJobOptions>(configuration.GetSection(RenewalJobOptions.SectionName));
services.Configure<NightlyExportJobOptions>(configuration.GetSection(NightlyExportJobOptions.SectionName));
services.AddSingleton<IJobClock, JobClock>();

// The renewal job calls the API's PolicyService, so it needs the same dependencies as the API.
services.AddSingleton<IMapper>(sp =>
{
    var expression = new MapperConfigurationExpression();
    expression.AddProfile<PolicyMappingProfile>();
    var mapperConfiguration = new MapperConfiguration(expression);
    return mapperConfiguration.CreateMapper();
});
services.AddScoped<PolicyService>();

services.AddSingleton<PremiumCalculator>();
services.AddSingleton<InstallmentScheduleCalculator>();
services.AddSingleton<PolicyNumberGenerator>();
services.AddSingleton<PolicyValidator>();
services.AddSingleton<RiskValidator>();
services.AddSingleton<PolicyHolderValidator>();
services.AddSingleton<InsuredPersonValidator>();
services.AddSingleton<EndorsementValidator>();
services.AddSingleton<EndorsementPremiumCalculator>();
services.AddSingleton<CancellationRefundCalculator>();
services.AddSingleton<CreditNoteService>();
services.AddSingleton<PolicyTermCalculator>();
services.AddSingleton<DriverAgeCalculator>();
services.AddSingleton<DiscountCalculator>();
services.AddSingleton<BusinessDayCalculator>();
services.AddSingleton<ClaimSlaService>();

// Job planning helpers
services.AddSingleton<BillingRunPlanner>();
services.AddSingleton<DunningPlanner>();
services.AddSingleton<SlaEscalationPlanner>();
services.AddSingleton<RenewalPlanner>();

// Jobs
services.AddHostedService<BillingRunJob>();
services.AddHostedService<DunningJob>();
services.AddHostedService<ClaimSlaEscalationJob>();
services.AddHostedService<RenewalJob>();
services.AddHostedService<NightlyExportJob>();

var host = builder.Build();

// Hosted services only start with host.Run, so the database exists and is seeded before any job runs.
using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ClaimHubDatabaseInitializer>();
    await initializer.InitializeAsync();
}

host.Run();
