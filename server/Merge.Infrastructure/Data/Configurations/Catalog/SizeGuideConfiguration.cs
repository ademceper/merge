using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class SizeGuideConfiguration : IEntityTypeConfiguration<SizeGuide>
{
    public void Configure(EntityTypeBuilder<SizeGuide> builder)
    {
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.IsActive);
        
        builder.HasOne(e => e.Category)
              .WithMany()
              .HasForeignKey(e => e.CategoryId)
              .OnDelete(DeleteBehavior.Restrict);
        
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.Entries)
              .WithOne(e => e.SizeGuide)
              .HasForeignKey(e => e.SizeGuideId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.ProductSizeGuides)
              .WithOne(e => e.SizeGuide)
              .HasForeignKey(e => e.SizeGuideId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
