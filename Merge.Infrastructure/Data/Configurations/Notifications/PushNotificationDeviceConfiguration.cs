using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class PushNotificationDeviceConfiguration : IEntityTypeConfiguration<PushNotificationDevice>
{
    public void Configure(EntityTypeBuilder<PushNotificationDevice> builder)
    {
        builder.Property(e => e.DeviceToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Platform)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.DeviceId)
            .HasMaxLength(200);

        builder.Property(e => e.DeviceModel)
            .HasMaxLength(100);

        builder.Property(e => e.AppVersion)
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.UserId, e.DeviceToken });
        builder.HasIndex(e => e.DeviceId);
    }
}
