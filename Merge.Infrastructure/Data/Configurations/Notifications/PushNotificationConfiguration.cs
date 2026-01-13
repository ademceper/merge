using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class PushNotificationConfiguration : IEntityTypeConfiguration<PushNotification>
{
    public void Configure(EntityTypeBuilder<PushNotification> builder)
    {
        builder.Property(e => e.NotificationType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Body)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(500);

        builder.HasIndex(e => new { e.UserId, e.Status });
        builder.HasIndex(e => e.DeviceId);
        builder.HasIndex(e => e.NotificationType);
    }
}
