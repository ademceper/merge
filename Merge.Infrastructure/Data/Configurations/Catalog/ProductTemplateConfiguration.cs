using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductTemplateConfiguration : IEntityTypeConfiguration<ProductTemplate>
{
    public void Configure(EntityTypeBuilder<ProductTemplate> builder)
    {
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.IsActive);
        
        builder.Property(e => e.DefaultPrice).HasPrecision(18, 2);
        
        builder.HasOne(e => e.Category)
              .WithMany()
              .HasForeignKey(e => e.CategoryId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
