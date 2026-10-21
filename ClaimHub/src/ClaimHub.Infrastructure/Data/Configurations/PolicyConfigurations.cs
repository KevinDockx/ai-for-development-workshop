using ClaimHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimHub.Infrastructure.Data.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(20);
        builder.Property(p => p.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.PaymentFrequency).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CancellationReason).HasMaxLength(500);
        builder.Property(p => p.AnnualPremium).HasPrecision(18, 2);
        builder.Property(p => p.TaxAmount).HasPrecision(18, 2);
        builder.Property(p => p.DiscountPercent).HasPrecision(5, 2);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);

        builder.HasIndex(p => p.PolicyNumber).IsUnique();
        builder.HasIndex(p => p.PolicyHolderId);
        builder.HasIndex(p => p.BrokerId);
        builder.HasIndex(p => new { p.Status, p.EndDate });

        builder.HasOne(p => p.PolicyHolder)
            .WithMany(h => h.Policies)
            .HasForeignKey(p => p.PolicyHolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Broker)
            .WithMany(b => b.Policies)
            .HasForeignKey(p => p.BrokerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.HomeRisk)
            .WithOne(r => r.Policy)
            .HasForeignKey<HomeRisk>(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MotorRisk)
            .WithOne(r => r.Policy)
            .HasForeignKey<MotorRisk>(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.TravelRisk)
            .WithOne(r => r.Policy)
            .HasForeignKey<TravelRisk>(r => r.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CoverageConfiguration : IEntityTypeConfiguration<Coverage>
{
    public void Configure(EntityTypeBuilder<Coverage> builder)
    {
        builder.ToTable("Coverages");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Limit).HasPrecision(18, 2);
        builder.Property(c => c.Deductible).HasPrecision(18, 2);
        builder.Property(c => c.Premium).HasPrecision(18, 2);

        builder.HasIndex(c => new { c.PolicyId, c.Type });

        builder.HasOne(c => c.Policy)
            .WithMany(p => p.Coverages)
            .HasForeignKey(c => c.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HomeRiskConfiguration : IEntityTypeConfiguration<HomeRisk>
{
    public void Configure(EntityTypeBuilder<HomeRisk> builder)
    {
        builder.ToTable("HomeRisks");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Street).IsRequired().HasMaxLength(120);
        builder.Property(r => r.PostalCode).IsRequired().HasMaxLength(10);
        builder.Property(r => r.City).IsRequired().HasMaxLength(80);
        builder.Property(r => r.BuildingValue).HasPrecision(18, 2);
        builder.Property(r => r.ContentsValue).HasPrecision(18, 2);

        builder.HasIndex(r => r.PolicyId).IsUnique();
    }
}

public class MotorRiskConfiguration : IEntityTypeConfiguration<MotorRisk>
{
    public void Configure(EntityTypeBuilder<MotorRisk> builder)
    {
        builder.ToTable("MotorRisks");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.LicensePlate).IsRequired().HasMaxLength(12);
        builder.Property(r => r.Make).IsRequired().HasMaxLength(60);
        builder.Property(r => r.Model).IsRequired().HasMaxLength(60);
        builder.Property(r => r.VehicleValue).HasPrecision(18, 2);

        builder.HasIndex(r => r.PolicyId).IsUnique();
        builder.HasIndex(r => r.LicensePlate);
    }
}

public class TravelRiskConfiguration : IEntityTypeConfiguration<TravelRisk>
{
    public void Configure(EntityTypeBuilder<TravelRisk> builder)
    {
        builder.ToTable("TravelRisks");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Scope).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.PartyType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(r => r.PolicyId).IsUnique();
    }
}

public class InsuredPersonConfiguration : IEntityTypeConfiguration<InsuredPerson>
{
    public void Configure(EntityTypeBuilder<InsuredPerson> builder)
    {
        builder.ToTable("InsuredPersons");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.FirstName).IsRequired().HasMaxLength(80);
        builder.Property(i => i.LastName).IsRequired().HasMaxLength(80);
        builder.Property(i => i.NationalNumber).HasMaxLength(16);
        builder.Property(i => i.Relationship).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(i => i.PolicyHolder)
            .WithMany(h => h.InsuredPersons)
            .HasForeignKey(i => i.PolicyHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Policy)
            .WithMany(p => p.InsuredPersons)
            .HasForeignKey(i => i.PolicyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PolicyEndorsementConfiguration : IEntityTypeConfiguration<PolicyEndorsement>
{
    public void Configure(EntityTypeBuilder<PolicyEndorsement> builder)
    {
        builder.ToTable("PolicyEndorsements");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.OldAnnualPremium).HasPrecision(18, 2);
        builder.Property(e => e.NewAnnualPremium).HasPrecision(18, 2);
        builder.Property(e => e.PremiumDelta).HasPrecision(18, 2);
        builder.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);

        builder.HasOne(e => e.Policy)
            .WithMany(p => p.Endorsements)
            .HasForeignKey(e => e.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RatingTableEntryConfiguration : IEntityTypeConfiguration<RatingTableEntry>
{
    public void Configure(EntityTypeBuilder<RatingTableEntry> builder)
    {
        builder.ToTable("RatingTable");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Factor).IsRequired().HasMaxLength(40);
        builder.Property(r => r.Key).IsRequired().HasMaxLength(40);
        builder.Property(r => r.Multiplier).HasPrecision(8, 4);

        builder.HasIndex(r => new { r.ProductType, r.Factor, r.Key, r.ValidFrom }).IsUnique();
    }
}
