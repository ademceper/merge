using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class AbandonedCartEmailConfiguration : IEntityTypeConfiguration<AbandonedCartEmail>
{
    public void Configure(EntityTypeBuilder<AbandonedCartEmail> builder)
    {
        builder.HasIndex(e => e.CartId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.CartId, e.UserId });

        builder.HasOne(e => e.Cart)
            .WithMany()
            .HasForeignKey(e => e.CartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Coupon)
            .WithMany()
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
