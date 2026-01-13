using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// PaymentMethod Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.IsDefault);
        builder.HasIndex(e => new { e.IsActive, e.DisplayOrder });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.IconUrl)
            .HasMaxLength(500);
        
        builder.Property(e => e.MinimumAmount)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.MaximumAmount)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.ProcessingFee)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.ProcessingFeePercentage)
            .HasPrecision(5, 2);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_PaymentMethod_MinAmount_NonNegative", "\"MinimumAmount\" IS NULL OR \"MinimumAmount\" >= 0");
            t.HasCheckConstraint("CK_PaymentMethod_MaxAmount_NonNegative", "\"MaximumAmount\" IS NULL OR \"MaximumAmount\" >= 0");
            t.HasCheckConstraint("CK_PaymentMethod_ProcessingFee_NonNegative", "\"ProcessingFee\" IS NULL OR \"ProcessingFee\" >= 0");
            t.HasCheckConstraint("CK_PaymentMethod_ProcessingFeePercentage_Range", "\"ProcessingFeePercentage\" IS NULL OR (\"ProcessingFeePercentage\" >= 0 AND \"ProcessingFeePercentage\" <= 100)");
            t.HasCheckConstraint("CK_PaymentMethod_MinMaxAmount", "\"MinimumAmount\" IS NULL OR \"MaximumAmount\" IS NULL OR \"MinimumAmount\" <= \"MaximumAmount\"");
        });
    }
}
