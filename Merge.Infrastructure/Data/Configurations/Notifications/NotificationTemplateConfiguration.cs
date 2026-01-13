using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100); // ✅ Domain'deki Guard.AgainstLength ile uyumlu

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.TitleTemplate)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.MessageTemplate)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.LinkTemplate)
            .HasMaxLength(500);

        builder.Property(e => e.Variables)
            .HasMaxLength(2000);

        builder.Property(e => e.DefaultData)
            .HasMaxLength(5000);

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => new { e.Type, e.IsActive });
    }
}
