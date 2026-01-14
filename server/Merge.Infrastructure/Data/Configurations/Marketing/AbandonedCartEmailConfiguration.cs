using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// AbandonedCartEmail EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class AbandonedCartEmailConfiguration : IEntityTypeConfiguration<AbandonedCartEmail>
{
    public void Configure(EntityTypeBuilder<AbandonedCartEmail> builder)
    {
        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.CartId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.CartId, e.UserId });

        // ✅ BOLUM 8.3: Relationship Configuration
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
