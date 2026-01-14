using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// FraudAlert Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class FraudAlertConfiguration : IEntityTypeConfiguration<FraudAlert>
{
    public void Configure(EntityTypeBuilder<FraudAlert> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.PaymentId);
        builder.HasIndex(e => e.AlertType);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.Status, e.AlertType });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Reason)
            .HasMaxLength(1000);
        
        builder.Property(e => e.ReviewNotes)
            .HasMaxLength(2000);
        
        builder.Property(e => e.MatchedRules)
            .HasColumnType("jsonb");
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FraudAlert_RiskScore_Range", "\"RiskScore\" >= 0 AND \"RiskScore\" <= 100");
        });
    }
}
