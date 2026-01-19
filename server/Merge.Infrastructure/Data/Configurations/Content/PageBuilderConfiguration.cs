using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;

namespace Merge.Infrastructure.Data.Configurations.Content;

public class PageBuilderConfiguration : IEntityTypeConfiguration<PageBuilder>
{
    public void Configure(EntityTypeBuilder<PageBuilder> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        
        // Slug value object conversion
        var slugConverter = new ValueConverter<Slug, string>(
            v => v.Value,
            v => new Slug(v));
        
        builder.Property(e => e.Slug)
            .HasConversion(slugConverter)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.HasIndex(e => e.Slug).IsUnique();
    }
}
