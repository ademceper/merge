using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.Property(e => e.NotificationType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Channel)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.CustomSettings)
            .HasMaxLength(5000);

        builder.HasIndex(e => new { e.UserId, e.NotificationType, e.Channel })
            .IsUnique();
    }
}
