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
        
        builder.Property(e => e.Data)
              .HasConversion(
                  v => v ?? string.Empty,
                  v => string.IsNullOrEmpty(v) ? null : v);
                  
        builder.HasIndex(e => new { e.UserId, e.IsRead });
    }
}
