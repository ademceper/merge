using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class PopularSearchConfiguration : IEntityTypeConfiguration<PopularSearch>
{
    public void Configure(EntityTypeBuilder<PopularSearch> builder)
    {
        builder.HasIndex(e => e.SearchTerm).IsUnique();
        builder.HasIndex(e => e.SearchCount);
        builder.HasIndex(e => e.LastSearchedAt);
        
        builder.Property(e => e.ClickThroughRate).HasPrecision(5, 2);
    }
}
