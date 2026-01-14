using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasOne(e => e.Order)
              .WithMany(e => e.OrderItems)
              .HasForeignKey(e => e.OrderId)
              .OnDelete(DeleteBehavior.Cascade);
              
        builder.HasOne(e => e.Product)
              .WithMany(e => e.OrderItems)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TotalPrice).HasPrecision(18, 2);
    }
}
