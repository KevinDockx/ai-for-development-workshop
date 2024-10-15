using ClaimHub.Infrastructure;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddClaimHubInfrastructure(builder.Configuration, options => options.UseLazyLoadingProxies());

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ClaimHub.Admin";
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
    .AddPolicy("Staff", policy => policy.RequireRole("Admin", "Underwriter", "ClaimsHandler", "SeniorClaimsHandler"));

builder.Services.AddRazorPages(options =>
{
    foreach (var folder in new[] { "/PolicyHolders", "/Policies", "/Claims", "/Billing", "/Settings" })
    {
        options.Conventions.AuthorizeFolder(folder, "Staff");
    }

    options.Conventions.AuthorizePage("/Dashboard", "Staff");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ClaimHubDatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
