using ClaimHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimHub.Infrastructure.Data.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.AssessedAmount).HasPrecision(18, 2);
        builder.Property(c => c.ApprovedAmount).HasPrecision(18, 2);
        builder.Property(c => c.PaidAmount).HasPrecision(18, 2);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);

        builder.HasIndex(c => c.ClaimNumber).IsUnique();
        builder.HasIndex(c => c.PolicyId);
        builder.HasIndex(c => new { c.Status, c.ReportedAt });

        builder.HasOne(c => c.Policy)
            .WithMany(p => p.Claims)
            .HasForeignKey(c => c.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Coverage)
            .WithMany(cv => cv.Claims)
            .HasForeignKey(c => c.CoverageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.AssignedHandler)
            .WithMany()
            .HasForeignKey(c => c.AssignedHandlerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ClaimStatusHistoryConfiguration : IEntityTypeConfiguration<ClaimStatusHistory>
{
    public void Configure(EntityTypeBuilder<ClaimStatusHistory> builder)
    {
        builder.ToTable("ClaimStatusHistory");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ChangedBy).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Note).HasMaxLength(500);

        builder.HasOne(h => h.Claim)
            .WithMany(c => c.StatusHistory)
            .HasForeignKey(h => h.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
{
    public void Configure(EntityTypeBuilder<ClaimDocument> builder)
    {
        builder.ToTable("ClaimDocuments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.FileName).IsRequired().HasMaxLength(260);
        builder.Property(d => d.StoredPath).IsRequired().HasMaxLength(520);
        builder.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.UploadedBy).IsRequired().HasMaxLength(200);

        builder.HasOne(d => d.Claim)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FraudFlagConfiguration : IEntityTypeConfiguration<FraudFlag>
{
    public void Configure(EntityTypeBuilder<FraudFlag> builder)
    {
        builder.ToTable("FraudFlags");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Type).HasConversion<string>().HasMaxLength(40);
        builder.Property(f => f.Details).IsRequired().HasMaxLength(500);
        builder.Property(f => f.ReviewedBy).HasMaxLength(200);

        builder.HasOne(f => f.Claim)
            .WithMany(c => c.FraudFlags)
            .HasForeignKey(f => f.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => new { i.PolicyId, i.InstallmentNumber, i.IssuedOn });
        builder.HasIndex(i => new { i.Status, i.DueDate });

        builder.HasOne(i => i.Policy)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => i.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Direction).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.GatewayReference).HasMaxLength(60);
        builder.Property(p => p.IdempotencyKey).HasMaxLength(100);
        builder.Property(p => p.FailureReason).HasMaxLength(500);

        builder.HasIndex(p => p.InvoiceId);
        builder.HasIndex(p => p.ClaimId);

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Claim)
            .WithMany()
            .HasForeignKey(p => p.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
