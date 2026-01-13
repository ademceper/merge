using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Payment;

namespace Merge.Infrastructure.Data.Configurations.Payment;

/// <summary>
/// GiftCard Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class GiftCardConfiguration : IEntityTypeConfiguration<GiftCard>
{
    public void Configure(EntityTypeBuilder<GiftCard> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.PurchasedByUserId);
        builder.HasIndex(e => e.AssignedToUserId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.IsRedeemed);
        builder.HasIndex(e => e.ExpiresAt);
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.RemainingAmount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.Message)
            .HasMaxLength(500);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_GiftCard_Amount_Positive", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_GiftCard_RemainingAmount_NonNegative", "\"RemainingAmount\" >= 0");
            t.HasCheckConstraint("CK_GiftCard_RemainingAmount_LessThanOrEqual_Amount", "\"RemainingAmount\" <= \"Amount\"");
        });
    }
}
