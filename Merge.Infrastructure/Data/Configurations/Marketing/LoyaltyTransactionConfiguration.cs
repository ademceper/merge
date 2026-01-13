using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// LoyaltyTransaction EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.LoyaltyAccountId);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => e.IsExpired);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => new { e.UserId, e.Type });
        builder.HasIndex(e => new { e.LoyaltyAccountId, e.IsExpired });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LoyaltyAccount)
            .WithMany()
            .HasForeignKey(e => e.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Review)
            .WithMany()
            .HasForeignKey(e => e.ReviewId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
