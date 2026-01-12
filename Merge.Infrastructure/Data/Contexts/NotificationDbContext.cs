using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Notifications;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

namespace Merge.Infrastructure.Data.Contexts;

public class NotificationDbContext : DbContext, IDbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<EmailAutomation> EmailAutomations { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<PushNotificationDevice> PushNotificationDevices { get; set; }
    public DbSet<PushNotification> PushNotifications { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users => throw new NotImplementedException();
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles => throw new NotImplementedException();
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Notifications");
    }
}
