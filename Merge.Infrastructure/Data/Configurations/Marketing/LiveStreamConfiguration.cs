using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

public class LiveStreamConfiguration : IEntityTypeConfiguration<LiveStream>
{
    public void Configure(EntityTypeBuilder<LiveStream> builder)
    {
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
    }
}
