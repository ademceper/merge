using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class ReferralCodeConfiguration : IEntityTypeConfiguration<ReferralCode>
{
    public void Configure(EntityTypeBuilder<ReferralCode> builder)
    {
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => new { e.UserId, e.IsActive });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ReferralCode_UsageCount_NonNegative", "\"UsageCount\" >= 0");
            t.HasCheckConstraint("CK_ReferralCode_MaxUsage_NonNegative", "\"MaxUsage\" >= 0");
            t.HasCheckConstraint("CK_ReferralCode_PointsReward_NonNegative", "\"PointsReward\" >= 0");
            t.HasCheckConstraint("CK_ReferralCode_DiscountPercentage_Range", "\"DiscountPercentage\" >= 0 AND \"DiscountPercentage\" <= 100");
        });
    }
}
