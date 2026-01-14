using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Content;

namespace Merge.Infrastructure.Data.Configurations.Content;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Position).HasMaxLength(50);
    }
}
