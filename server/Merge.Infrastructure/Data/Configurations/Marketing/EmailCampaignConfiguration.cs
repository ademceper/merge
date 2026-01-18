using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class EmailCampaignConfiguration : IEntityTypeConfiguration<EmailCampaign>
{
    public void Configure(EntityTypeBuilder<EmailCampaign> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FromName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FromEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ReplyToEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasMaxLength(50000);

        builder.Property(e => e.TargetSegment)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Tags)
            .HasMaxLength(1000);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.ScheduledAt);
        builder.HasIndex(e => e.SentAt);
        builder.HasIndex(e => new { e.Status, e.Type });
        builder.HasIndex(e => new { e.Status, e.ScheduledAt });

        builder.HasOne(e => e.Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Recipients)
            .WithOne(r => r.Campaign)
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_EmailCampaign_TotalRecipients_NonNegative", "\"TotalRecipients\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaign_SentCount_NonNegative", "\"SentCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaign_DeliveredCount_NonNegative", "\"DeliveredCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaign_OpenedCount_NonNegative", "\"OpenedCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaign_ClickedCount_NonNegative", "\"ClickedCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaign_BouncedCount_NonNegative", "\"BouncedCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaign_UnsubscribedCount_NonNegative", "\"UnsubscribedCount\" >= 0");
            t.HasCheckConstraint("CK_EmailCampaign_OpenRate_Range", "\"OpenRate\" >= 0 AND \"OpenRate\" <= 100");
            t.HasCheckConstraint("CK_EmailCampaign_ClickRate_Range", "\"ClickRate\" >= 0 AND \"ClickRate\" <= 100");
        });
    }
}
