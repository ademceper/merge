using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;


public class InternationalShippingConfiguration : IEntityTypeConfiguration<InternationalShipping>
{
    public void Configure(EntityTypeBuilder<InternationalShipping> builder)
    {
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.TrackingNumber);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.OriginCountry, e.DestinationCountry });
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.OriginCountry)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.DestinationCountry)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.OriginCity)
            .HasMaxLength(100);
        
        builder.Property(e => e.DestinationCity)
            .HasMaxLength(100);
        
        builder.Property(e => e.ShippingMethod)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.ShippingCost)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.CustomsDuty)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.ImportTax)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.HandlingFee)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.TrackingNumber)
            .HasMaxLength(100);
        
        builder.Property(e => e.CustomsDeclarationNumber)
            .HasMaxLength(100);
        
        // Navigation properties
        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InternationalShipping_ShippingCost_NonNegative", "\"ShippingCost\" >= 0");
            t.HasCheckConstraint("CK_InternationalShipping_CustomsDuty_NonNegative", "\"CustomsDuty\" IS NULL OR \"CustomsDuty\" >= 0");
            t.HasCheckConstraint("CK_InternationalShipping_ImportTax_NonNegative", "\"ImportTax\" IS NULL OR \"ImportTax\" >= 0");
            t.HasCheckConstraint("CK_InternationalShipping_HandlingFee_NonNegative", "\"HandlingFee\" IS NULL OR \"HandlingFee\" >= 0");
            t.HasCheckConstraint("CK_InternationalShipping_TotalCost_NonNegative", "\"TotalCost\" >= 0");
            t.HasCheckConstraint("CK_InternationalShipping_EstimatedDays_NonNegative", "\"EstimatedDays\" >= 0");
        });
    }
}
