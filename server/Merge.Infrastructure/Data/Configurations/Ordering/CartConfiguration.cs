using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasIndex(e => e.UserId);

        // ✅ CRITICAL-DB-002 FIX: RowVersion configuration for optimistic concurrency
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);

        builder.HasOne(e => e.User)
              .WithMany(e => e.Carts)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
