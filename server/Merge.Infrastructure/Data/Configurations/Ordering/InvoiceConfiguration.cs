using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasIndex(e => e.InvoiceNumber).IsUnique();
        builder.HasOne(e => e.Order)
              .WithOne(e => e.Invoice)
              .HasForeignKey<Invoice>(e => e.OrderId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.InvoiceDate);
        
        builder.Property(e => e.SubTotal).HasPrecision(18, 2);
        builder.Property(e => e.Tax).HasPrecision(18, 2);
        builder.Property(e => e.ShippingCost).HasPrecision(18, 2);
        builder.Property(e => e.Discount).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Invoice_SubTotal_NonNegative", "\"SubTotal\" >= 0");
            t.HasCheckConstraint("CK_Invoice_Tax_NonNegative", "\"Tax\" >= 0");
            t.HasCheckConstraint("CK_Invoice_ShippingCost_NonNegative", "\"ShippingCost\" >= 0");
            t.HasCheckConstraint("CK_Invoice_Discount_NonNegative", "\"Discount\" >= 0");
            t.HasCheckConstraint("CK_Invoice_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
        });
    }
}
