using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClaimHub.Infrastructure.Data;

/// <summary>Lets the EF Core tooling create the context without starting a host.</summary>
public class ClaimHubDbContextFactory : IDesignTimeDbContextFactory<ClaimHubDbContext>
{
    public ClaimHubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClaimHubDbContext>()
            .UseSqlite(DatabasePaths.ResolveConnectionString("Data Source=data/claimhub.db"))
            .Options;

        return new ClaimHubDbContext(options);
    }
}
