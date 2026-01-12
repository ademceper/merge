using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
    }
}
