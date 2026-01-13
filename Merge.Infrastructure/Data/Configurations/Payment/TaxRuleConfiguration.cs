using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// TaxRule Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.Country);
        builder.HasIndex(e => e.TaxType);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.Country, e.State, e.City });
        builder.HasIndex(e => new { e.Country, e.IsActive });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Country)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.State)
            .HasMaxLength(100);
        
        builder.Property(e => e.City)
            .HasMaxLength(100);
        
        builder.Property(e => e.TaxRate)
            .HasPrecision(5, 2)
            .IsRequired();
        
        builder.Property(e => e.ProductCategoryIds)
            .HasMaxLength(1000);
        
        builder.Property(e => e.Notes)
            .HasMaxLength(1000);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_TaxRule_TaxRate_Range", "\"TaxRate\" >= 0 AND \"TaxRate\" <= 100");
            t.HasCheckConstraint("CK_TaxRule_EffectiveDates", "\"EffectiveFrom\" IS NULL OR \"EffectiveTo\" IS NULL OR \"EffectiveFrom\" <= \"EffectiveTo\"");
        });
    }
}
