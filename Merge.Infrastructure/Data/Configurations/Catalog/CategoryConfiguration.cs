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
              .WithMany(e => e.SubCategories)
              .HasForeignKey(e => e.ParentCategoryId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
