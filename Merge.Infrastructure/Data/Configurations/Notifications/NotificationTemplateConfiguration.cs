using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Notifications;

namespace Merge.Infrastructure.Data.Configurations.Notifications;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Type)
              .IsRequired()
              .HasConversion<string>();
        builder.Property(e => e.TitleTemplate).IsRequired().HasMaxLength(500);
        builder.Property(e => e.MessageTemplate).IsRequired();
        builder.HasIndex(e => e.Type);
    }
}
