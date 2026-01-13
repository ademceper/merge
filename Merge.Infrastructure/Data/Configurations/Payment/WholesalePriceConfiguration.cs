using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// WholesalePrice Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class WholesalePriceConfiguration : IEntityTypeConfiguration<WholesalePrice>
{
    public void Configure(EntityTypeBuilder<WholesalePrice> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.ProductId, e.OrganizationId });
        builder.HasIndex(e => new { e.ProductId, e.IsActive });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_WholesalePrice_Price_Positive", "\"Price\" > 0");
            t.HasCheckConstraint("CK_WholesalePrice_MinMaxQuantity", "\"MaxQuantity\" IS NULL OR \"MaxQuantity\" >= \"MinQuantity\"");
            t.HasCheckConstraint("CK_WholesalePrice_StartEndDate", "\"StartDate\" IS NULL OR \"EndDate\" IS NULL OR \"StartDate\" <= \"EndDate\"");
        });
    }
}
