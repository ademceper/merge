using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasIndex(e => e.Slug).IsUnique();
        
        builder.HasOne(e => e.ParentCategory)
              .WithMany()
              .HasForeignKey(e => e.ParentCategoryId)
              .OnDelete(DeleteBehavior.Restrict);
        
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.SubCategories)
              .WithOne(e => e.ParentCategory)
              .HasForeignKey(e => e.ParentCategoryId)
              .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.Products)
              .WithOne(e => e.Category)
              .HasForeignKey(e => e.CategoryId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
