using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// EmailCampaignRecipient EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class EmailCampaignRecipientConfiguration : IEntityTypeConfiguration<EmailCampaignRecipient>
{
    public void Configure(EntityTypeBuilder<EmailCampaignRecipient> builder)
    {
        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.CampaignId);
        builder.HasIndex(e => e.SubscriberId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.CampaignId, e.SubscriberId }).IsUnique();
        builder.HasIndex(e => new { e.CampaignId, e.Status });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.Campaign)
            .WithMany(c => c.Recipients)
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Subscriber)
            .WithMany()
            .HasForeignKey(e => e.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_EmailCampaignRecipient_OpenCount_NonNegative", "\"OpenCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaignRecipient_ClickCount_NonNegative", "\"ClickCount\" >= 0");
        });
    }
}
