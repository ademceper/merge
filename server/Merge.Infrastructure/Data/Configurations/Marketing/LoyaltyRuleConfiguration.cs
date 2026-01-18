using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class LoyaltyRuleConfiguration : IEntityTypeConfiguration<LoyaltyRule>
{
    public void Configure(EntityTypeBuilder<LoyaltyRule> builder)
    {
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

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.Type, e.IsActive });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LoyaltyRule_PointsAwarded_NonNegative", "\"PointsAwarded\" >= 0");
            t.HasCheckConstraint("CK_LoyaltyRule_MinimumPurchaseAmount_NonNegative", "\"MinimumPurchaseAmount\" IS NULL OR \"MinimumPurchaseAmount\" >= 0");
            t.HasCheckConstraint("CK_LoyaltyRule_ExpiryDays_Positive", "\"ExpiryDays\" IS NULL OR \"ExpiryDays\" > 0");
        });
    }
}
