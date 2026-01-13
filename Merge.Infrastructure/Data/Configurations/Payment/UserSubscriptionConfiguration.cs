using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// UserSubscription Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.SubscriptionPlanId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.UserId, e.Status });
        builder.HasIndex(e => e.NextBillingDate);
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.CurrentPrice)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.PaymentMethodId)
            .HasMaxLength(255);
        
        builder.Property(e => e.CancellationReason)
            .HasMaxLength(1000);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_UserSubscription_CurrentPrice_NonNegative", "\"CurrentPrice\" >= 0");
            t.HasCheckConstraint("CK_UserSubscription_RenewalCount_NonNegative", "\"RenewalCount\" >= 0");
            t.HasCheckConstraint("CK_UserSubscription_StartEndDate", "\"StartDate\" <= \"EndDate\"");
            t.HasCheckConstraint("CK_UserSubscription_TrialEndDate", "\"TrialEndDate\" IS NULL OR \"TrialEndDate\" >= \"StartDate\"");
        });
    }
}
