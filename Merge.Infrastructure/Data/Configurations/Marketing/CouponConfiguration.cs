using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
        builder.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(e => e.MinimumPurchaseAmount).HasPrecision(18, 2);
        builder.Property(e => e.MaximumDiscountAmount).HasPrecision(18, 2);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Coupon_DiscountPercentage_Range", "\"DiscountPercentage\" >= 0 AND \"DiscountPercentage\" <= 100");
            t.HasCheckConstraint("CK_Coupon_DiscountAmount_Positive", "\"DiscountAmount\" >= 0");
            t.HasCheckConstraint("CK_Coupon_UsageCount_NonNegative", "\"UsedCount\" >= 0");
            t.HasCheckConstraint("CK_Coupon_DiscountAmount_NonNegative", "\"DiscountAmount\" >= 0");
            t.HasCheckConstraint("CK_Coupon_DiscountPercentage_Range2", "\"DiscountPercentage\" IS NULL OR (\"DiscountPercentage\" >= 0 AND \"DiscountPercentage\" <= 100)");
            t.HasCheckConstraint("CK_Coupon_UsedCount_LessThan_UsageLimit", "\"UsedCount\" <= \"UsageLimit\" OR \"UsageLimit\" = 0");
        });

        // ✅ BOLUM 1.1: Rich Domain Model - Backing field mapping for encapsulated collections
        var listGuidToStringConverter = new ValueConverter<List<Guid>?, string?>(
            v => v != null ? string.Join(',', v) : null,
            v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList() : null);

        builder.Property("_applicableCategoryIds")
              .HasConversion(listGuidToStringConverter);

        builder.Property("_applicableProductIds")
              .HasConversion(listGuidToStringConverter);
    }
}
