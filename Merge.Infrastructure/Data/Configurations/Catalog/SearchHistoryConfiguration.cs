using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistory>
{
    public void Configure(EntityTypeBuilder<SearchHistory> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.SearchTerm);
        builder.HasIndex(e => e.ClickedProductId);
        builder.HasIndex(e => new { e.UserId, e.SearchTerm });
        
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.ClickedProduct)
              .WithMany()
              .HasForeignKey(e => e.ClickedProductId)
              .OnDelete(DeleteBehavior.SetNull);
    }
}
