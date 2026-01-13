using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// FlashSale EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.BannerImageUrl)
            .HasMaxLength(500);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => e.EndDate);
        builder.HasIndex(e => new { e.IsActive, e.StartDate, e.EndDate });

        // ✅ BOLUM 8.3: Relationship Configuration
        // ✅ BOLUM 1.1: Rich Domain Model - Backing field mapping for encapsulated collection
        builder.HasMany(e => e.FlashSaleProducts)
            .WithOne(p => p.FlashSale)
            .HasForeignKey(p => p.FlashSaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FlashSale_EndDate_After_StartDate", "\"EndDate\" > \"StartDate\"");
        });
    }
}
