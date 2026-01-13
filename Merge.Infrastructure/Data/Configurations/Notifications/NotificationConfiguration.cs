using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Link)
            .HasMaxLength(500);

        builder.Property(e => e.Data)
            .HasConversion(
                v => v ?? string.Empty,
                v => string.IsNullOrEmpty(v) ? null : v);

        builder.HasIndex(e => new { e.UserId, e.IsRead });
        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        builder.HasIndex(e => e.Type);
    }
}
