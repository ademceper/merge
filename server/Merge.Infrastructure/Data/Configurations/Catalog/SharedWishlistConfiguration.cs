using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class SharedWishlistConfiguration : IEntityTypeConfiguration<SharedWishlist>
{
    public void Configure(EntityTypeBuilder<SharedWishlist> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ShareCode).IsUnique().HasFilter("\"ShareCode\" != ''");
        
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        
        // ✅ BOLUM 1.1: Backing field mapping for encapsulated collections
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.Items)
              .WithOne(e => e.SharedWishlist)
              .HasForeignKey(e => e.SharedWishlistId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
