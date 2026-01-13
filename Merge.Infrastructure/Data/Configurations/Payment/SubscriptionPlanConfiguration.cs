using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// SubscriptionPlan Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.DisplayOrder);
        builder.HasIndex(e => new { e.IsActive, e.DisplayOrder });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
        
        builder.Property(e => e.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.SetupFee)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();
        
        builder.Property(e => e.Features)
            .HasColumnType("jsonb");
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SubscriptionPlan_Price_NonNegative", "\"Price\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_SetupFee_NonNegative", "\"SetupFee\" IS NULL OR \"SetupFee\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_DurationDays_Positive", "\"DurationDays\" > 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_TrialDays_NonNegative", "\"TrialDays\" IS NULL OR \"TrialDays\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_MaxUsers_Positive", "\"MaxUsers\" > 0");
            t.HasCheckConstraint("CK_SubscriptionPlan_DisplayOrder_NonNegative", "\"DisplayOrder\" >= 0");
        });
    }
}
