using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Support;
using Merge.Domain.Modules.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;
using User = Merge.Domain.Modules.Identity.User;
using Role = Merge.Domain.Modules.Identity.Role;

namespace Merge.Infrastructure.Data.Contexts;

public class SupportDbContext(DbContextOptions<SupportDbContext> options) : DbContext(options), IDbContext
{

    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    public DbSet<CustomerCommunication> CustomerCommunications { get; set; }
    public DbSet<LiveChatSession> LiveChatSessions { get; set; }
    public DbSet<LiveChatMessage> LiveChatMessages { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    DbSet<User> IDbContext.Users =>
        throw new InvalidOperationException("SupportDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Role> IDbContext.Roles =>
        throw new InvalidOperationException("SupportDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles =>
        throw new InvalidOperationException("SupportDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Support");

        ConfigureGlobalQueryFilters(modelBuilder);
    }

    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
