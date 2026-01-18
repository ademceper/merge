using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasIndex(e => e.OrderNumber).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.UserId, e.Status });
        
        builder.HasIndex(e => e.AddressId);
        builder.HasIndex(e => e.ParentOrderId);
        
        builder.Property(e => e.SubTotal).HasPrecision(18, 2);
        builder.Property(e => e.ShippingCost).HasPrecision(18, 2);
        builder.Property(e => e.Tax).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Order_TotalAmount_Positive", "\"TotalAmount\" >= 0");
            t.HasCheckConstraint("CK_Order_SubTotal_Positive", "\"SubTotal\" >= 0");
            t.HasCheckConstraint("CK_Order_ShippingCost_Positive", "\"ShippingCost\" >= 0");
            t.HasCheckConstraint("CK_Order_Tax_Positive", "\"Tax\" >= 0");
        });
        
        builder.HasOne(e => e.User)
              .WithMany(e => e.Orders)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasOne(e => e.Address)
              .WithMany(e => e.Orders)
              .HasForeignKey(e => e.AddressId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasOne(e => e.ParentOrder)
              .WithMany(e => e.SplitOrders)
              .HasForeignKey(e => e.ParentOrderId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
