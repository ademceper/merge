using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.HasOne(e => e.Coupon)
              .WithMany()
              .HasForeignKey(e => e.CouponId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
              .WithMany(e => e.CouponUsages)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Order)
              .WithMany()
              .HasForeignKey(e => e.OrderId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
    }
}
