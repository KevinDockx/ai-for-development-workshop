using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using ClaimHub.Api.Endpoints;
using ClaimHub.Api.Features.Claims.Handlers;
using ClaimHub.Api.OpenApi;
using ClaimHub.Api.Policies;
using ClaimHub.Api.Security;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Mediator;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddClaimHubInfrastructure(builder.Configuration);

// Authentication
services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
services.AddSingleton<IJwtTokenService, JwtTokenService>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtTokenService.BuildKey(jwtSettings.SigningKey),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            NameClaimType = "email",
            RoleClaimType = ClaimsPrincipalExtensions.RoleClaim
        };
    });

services.AddAuthorizationBuilder()
    .AddPolicy("ClaimsStaff", policy => policy.RequireRole("ClaimsHandler", "SeniorClaimsHandler", "Admin"));

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// MVC and JSON
services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecurityTransformer>());

// Policies area
services.AddSingleton<IMapper>(sp =>
{
    var expression = new MapperConfigurationExpression();
    expression.AddProfile<PolicyMappingProfile>();
    var configuration = new MapperConfiguration(expression);
    return configuration.CreateMapper();
});
services.AddScoped<PolicyService>();

// Domain services
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
services.AddScoped<ClaimNumberGenerator>();
services.AddSingleton<ClaimStateMachine>();
services.AddSingleton<ClaimEligibilityService>();
services.AddSingleton<ClaimInputValidator>();
services.AddSingleton<ClaimSlaService>();
services.AddSingleton<FraudDetectionService>();
services.AddSingleton<ClaimPayoutCalculator>();
services.AddSingleton<CoverageLimitService>();
services.AddSingleton<ClaimReserveService>();
services.AddSingleton<ClaimTimelineBuilder>();

// Claims area
services.AddValidatorsFromAssemblyContaining<Program>();
services.AddMiniMediator(typeof(Program).Assembly);
services.AddScoped<ClaimDetailBuilder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ClaimHubDatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.UseDeveloperExceptionPage();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapClaimsEndpoints();
app.MapDocumentEndpoints();
app.MapReportEndpoints();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("ClaimHub API"));

app.MapGet("/", () => "ClaimHub API");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program;

