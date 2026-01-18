using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;


public class VolumeDiscountConfiguration : IEntityTypeConfiguration<VolumeDiscount>
{
    public void Configure(EntityTypeBuilder<VolumeDiscount> builder)
    {
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.ProductId, e.IsActive });
        builder.HasIndex(e => new { e.CategoryId, e.IsActive });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.DiscountPercentage)
            .HasPrecision(5, 2)
            .IsRequired();
        
        builder.Property(e => e.FixedDiscountAmount)
            .HasPrecision(18, 2);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_VolumeDiscount_DiscountPercentage_Range", "\"DiscountPercentage\" >= 0 AND \"DiscountPercentage\" <= 100");
            t.HasCheckConstraint("CK_VolumeDiscount_FixedDiscountAmount_NonNegative", "\"FixedDiscountAmount\" IS NULL OR \"FixedDiscountAmount\" >= 0");
            t.HasCheckConstraint("CK_VolumeDiscount_MinMaxQuantity", "\"MaxQuantity\" IS NULL OR \"MaxQuantity\" >= \"MinQuantity\"");
            t.HasCheckConstraint("CK_VolumeDiscount_StartEndDate", "\"StartDate\" IS NULL OR \"EndDate\" IS NULL OR \"StartDate\" <= \"EndDate\"");
        });
    }
}
