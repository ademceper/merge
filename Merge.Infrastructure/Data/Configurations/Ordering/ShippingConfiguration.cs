using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class ShippingConfiguration : IEntityTypeConfiguration<Shipping>
{
    public void Configure(EntityTypeBuilder<Shipping> builder)
    {
        builder.HasOne(e => e.Order)
              .WithOne(e => e.Shipping)
              .HasForeignKey<Shipping>(e => e.OrderId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.TrackingNumber);
        builder.HasIndex(e => e.Status);
        
        builder.Property(e => e.ShippingCost).HasPrecision(18, 2);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Shipping_ShippingCost_NonNegative", "\"ShippingCost\" >= 0");
        });
    }
}
