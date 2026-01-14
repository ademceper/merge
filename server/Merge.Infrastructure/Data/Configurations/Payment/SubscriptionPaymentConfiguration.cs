using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// SubscriptionPayment Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.UserSubscriptionId);
        builder.HasIndex(e => e.PaymentStatus);
        builder.HasIndex(e => e.TransactionId);
        builder.HasIndex(e => new { e.UserSubscriptionId, e.PaymentStatus });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.TransactionId)
            .HasMaxLength(255);
        
        builder.Property(e => e.FailureReason)
            .HasMaxLength(1000);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SubscriptionPayment_Amount_Positive", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_SubscriptionPayment_RetryCount_NonNegative", "\"RetryCount\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionPayment_BillingPeriod", "\"BillingPeriodStart\" < \"BillingPeriodEnd\"");
        });
    }
}
