using ClaimHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Data;

public class ClaimHubDbContext : DbContext
{
    public ClaimHubDbContext(DbContextOptions<ClaimHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<PolicyHolder> PolicyHolders { get; set; }

    public DbSet<Broker> Brokers { get; set; }

    public DbSet<Policy> Policies { get; set; }

    public DbSet<Coverage> Coverages { get; set; }

    public DbSet<HomeRisk> HomeRisks { get; set; }

    public DbSet<MotorRisk> MotorRisks { get; set; }

    public DbSet<TravelRisk> TravelRisks { get; set; }

    public DbSet<InsuredPerson> InsuredPersons { get; set; }

    public DbSet<PolicyEndorsement> PolicyEndorsements { get; set; }

    public DbSet<Claim> Claims { get; set; }

    public DbSet<ClaimStatusHistory> ClaimStatusHistory { get; set; }

    public DbSet<ClaimDocument> ClaimDocuments { get; set; }

    public DbSet<FraudFlag> FraudFlags { get; set; }

    public DbSet<Invoice> Invoices { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<ApplicationUser> Users { get; set; }

    public DbSet<RatingTableEntry> RatingTable { get; set; }

    public DbSet<AuditEntry> AuditEntries { get; set; }

    public DbSet<NumberSequence> NumberSequences { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite cannot order or compare DateTimeOffset values stored as text, so store UTC ticks.
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetTicksConverter>();
        configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<NullableDateTimeOffsetTicksConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimHubDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
