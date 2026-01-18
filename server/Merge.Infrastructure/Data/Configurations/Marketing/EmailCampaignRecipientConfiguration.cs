using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class EmailCampaignRecipientConfiguration : IEntityTypeConfiguration<EmailCampaignRecipient>
{
    public void Configure(EntityTypeBuilder<EmailCampaignRecipient> builder)
    {
        builder.HasIndex(e => e.CampaignId);
        builder.HasIndex(e => e.SubscriberId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.CampaignId, e.SubscriberId }).IsUnique();
        builder.HasIndex(e => new { e.CampaignId, e.Status });

        builder.HasOne(e => e.Campaign)
            .WithMany(c => c.Recipients)
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Subscriber)
            .WithMany()
            .HasForeignKey(e => e.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_EmailCampaignRecipient_OpenCount_NonNegative", "\"OpenCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaignRecipient_ClickCount_NonNegative", "\"ClickCount\" >= 0");
        });
    }
}
