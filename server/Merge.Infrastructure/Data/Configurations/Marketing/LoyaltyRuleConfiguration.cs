using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// LoyaltyRule EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class LoyaltyRuleConfiguration : IEntityTypeConfiguration<LoyaltyRule>
{
    public void Configure(EntityTypeBuilder<LoyaltyRule> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.MinimumPurchaseAmount)
            .HasPrecision(18, 2);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.Type, e.IsActive });

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LoyaltyRule_PointsAwarded_NonNegative", "\"PointsAwarded\" >= 0");
            t.HasCheckConstraint("CK_LoyaltyRule_MinimumPurchaseAmount_NonNegative", "\"MinimumPurchaseAmount\" IS NULL OR \"MinimumPurchaseAmount\" >= 0");
            t.HasCheckConstraint("CK_LoyaltyRule_ExpiryDays_Positive", "\"ExpiryDays\" IS NULL OR \"ExpiryDays\" > 0");
        });
    }
}
