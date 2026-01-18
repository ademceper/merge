using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;


public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.SubscriptionPlanId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.UserId, e.Status });
        builder.HasIndex(e => e.NextBillingDate);
        
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
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_UserSubscription_CurrentPrice_NonNegative", "\"CurrentPrice\" >= 0");
            t.HasCheckConstraint("CK_UserSubscription_RenewalCount_NonNegative", "\"RenewalCount\" >= 0");
            t.HasCheckConstraint("CK_UserSubscription_StartEndDate", "\"StartDate\" <= \"EndDate\"");
            t.HasCheckConstraint("CK_UserSubscription_TrialEndDate", "\"TrialEndDate\" IS NULL OR \"TrialEndDate\" >= \"StartDate\"");
        });
    }
}
