using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Support;

namespace Merge.Infrastructure.Data.Configurations.Support;

public class FAQConfiguration : IEntityTypeConfiguration<FAQ>
{
    public void Configure(EntityTypeBuilder<FAQ> builder)
    {
        builder.Property(e => e.Question).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Answer).IsRequired().HasMaxLength(5000);
        builder.Property(e => e.Category).HasMaxLength(50);
    }
}
