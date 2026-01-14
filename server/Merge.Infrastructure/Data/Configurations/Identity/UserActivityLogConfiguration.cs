using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.SetNull);
              
        builder.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IpAddress).HasMaxLength(50);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        
        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        builder.HasIndex(e => new { e.ActivityType, e.CreatedAt });
    }
}
