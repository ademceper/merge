using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.LoyaltyAccountId);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => e.IsExpired);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => new { e.UserId, e.Type });
        builder.HasIndex(e => new { e.LoyaltyAccountId, e.IsExpired });

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
