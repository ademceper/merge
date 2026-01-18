using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;


public class PaymentFraudPreventionConfiguration : IEntityTypeConfiguration<PaymentFraudPrevention>
{
    public void Configure(EntityTypeBuilder<PaymentFraudPrevention> builder)
    {
        builder.HasIndex(e => e.PaymentId);
        builder.HasIndex(e => e.CheckType);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsBlocked);
        builder.HasIndex(e => new { e.PaymentId, e.CheckType });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.BlockReason)
            .HasMaxLength(1000);
        
        builder.Property(e => e.CheckResult)
            .HasColumnType("jsonb");
        
        builder.Property(e => e.DeviceFingerprint)
            .HasMaxLength(255);
        
        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);
        
        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_PaymentFraudPrevention_RiskScore_Range", "\"RiskScore\" >= 0 AND \"RiskScore\" <= 100");
        });
    }
}
