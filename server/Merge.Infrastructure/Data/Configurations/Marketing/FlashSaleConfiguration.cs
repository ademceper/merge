using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.BannerImageUrl)
            .HasMaxLength(500);

        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => e.EndDate);
        builder.HasIndex(e => new { e.IsActive, e.StartDate, e.EndDate });

        builder.HasMany(e => e.FlashSaleProducts)
            .WithOne(p => p.FlashSale)
            .HasForeignKey(p => p.FlashSaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FlashSale_EndDate_After_StartDate", "\"EndDate\" > \"StartDate\"");
        });
    }
}
