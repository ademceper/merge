using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// LoyaltyTier EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class LoyaltyTierConfiguration : IEntityTypeConfiguration<LoyaltyTier>
{
    public void Configure(EntityTypeBuilder<LoyaltyTier> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.DiscountPercentage)
            .HasPrecision(5, 2);

        builder.Property(e => e.PointsMultiplier)
            .HasPrecision(5, 2);

        builder.Property(e => e.Color)
            .HasMaxLength(50);

        builder.Property(e => e.IconUrl)
            .HasMaxLength(500);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.Level).IsUnique();
        builder.HasIndex(e => e.MinimumPoints);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.Level, e.IsActive });

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LoyaltyTier_MinimumPoints_NonNegative", "\"MinimumPoints\" >= 0");
            t.HasCheckConstraint("CK_LoyaltyTier_DiscountPercentage_Range", "\"DiscountPercentage\" >= 0 AND \"DiscountPercentage\" <= 100");
            t.HasCheckConstraint("CK_LoyaltyTier_PointsMultiplier_Positive", "\"PointsMultiplier\" > 0");
            t.HasCheckConstraint("CK_LoyaltyTier_Level_Positive", "\"Level\" > 0");
        });
    }
}
