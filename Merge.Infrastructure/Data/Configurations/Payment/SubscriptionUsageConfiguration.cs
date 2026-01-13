using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// SubscriptionUsage Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class SubscriptionUsageConfiguration : IEntityTypeConfiguration<SubscriptionUsage>
{
    public void Configure(EntityTypeBuilder<SubscriptionUsage> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.UserSubscriptionId);
        builder.HasIndex(e => e.Feature);
        builder.HasIndex(e => new { e.UserSubscriptionId, e.Feature });
        builder.HasIndex(e => new { e.UserSubscriptionId, e.PeriodStart, e.PeriodEnd });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Feature)
            .IsRequired()
            .HasMaxLength(100);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SubscriptionUsage_UsageCount_NonNegative", "\"UsageCount\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionUsage_Limit_NonNegative", "\"Limit\" IS NULL OR \"Limit\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionUsage_Period", "\"PeriodStart\" < \"PeriodEnd\"");
        });
    }
}
