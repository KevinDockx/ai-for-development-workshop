using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimHub.Infrastructure.Data;

/// <summary>
/// One row per number series, for example "Policy:HOM-2025" or "Claim:2025". The row holds the last number handed out,
/// so every host that shares the SQLite file draws from the same counter.
/// </summary>
public class NumberSequence
{
    public string Name { get; set; }

    public int LastValue { get; set; }
}

public class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable("NumberSequences");
        builder.HasKey(s => s.Name);
        builder.Property(s => s.Name).HasMaxLength(64);
    }
}
