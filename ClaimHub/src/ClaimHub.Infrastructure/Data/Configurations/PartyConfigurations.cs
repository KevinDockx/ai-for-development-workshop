using ClaimHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimHub.Infrastructure.Data.Configurations;

public class PolicyHolderConfiguration : IEntityTypeConfiguration<PolicyHolder>
{
    public void Configure(EntityTypeBuilder<PolicyHolder> builder)
    {
        builder.ToTable("PolicyHolders");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FirstName).IsRequired().HasMaxLength(80);
        builder.Property(h => h.LastName).IsRequired().HasMaxLength(80);
        builder.Property(h => h.Email).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Phone).HasMaxLength(30);
        builder.Property(h => h.NationalNumber).IsRequired().HasMaxLength(16);
        builder.Property(h => h.Iban).IsRequired().HasMaxLength(34);
        builder.Property(h => h.Street).IsRequired().HasMaxLength(120);
        builder.Property(h => h.PostalCode).IsRequired().HasMaxLength(10);
        builder.Property(h => h.City).IsRequired().HasMaxLength(80);
        builder.Property(h => h.Role).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.CreditLimit).HasPrecision(18, 2);
        builder.Property(h => h.DeletedBy).HasMaxLength(200);

        builder.HasIndex(h => h.Email);
        builder.HasIndex(h => h.NationalNumber);
        builder.HasIndex(h => new { h.LastName, h.FirstName });

        builder.HasOne(h => h.Broker)
            .WithMany(b => b.PolicyHolders)
            .HasForeignKey(h => h.BrokerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class BrokerConfiguration : IEntityTypeConfiguration<Broker>
{
    public void Configure(EntityTypeBuilder<Broker> builder)
    {
        builder.ToTable("Brokers");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.RegistrationNumber).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Email).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Phone).HasMaxLength(30);
        builder.Property(b => b.CommissionRate).HasPrecision(5, 4);

        builder.HasIndex(b => b.RegistrationNumber).IsUnique();
    }
}

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(64);
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.PolicyHolder)
            .WithMany()
            .HasForeignKey(u => u.PolicyHolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Broker)
            .WithMany(b => b.Users)
            .HasForeignKey(u => u.BrokerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(60);
        builder.Property(a => a.UserName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Details).HasMaxLength(2000);

        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
    }
}
