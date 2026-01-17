using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;
using Merge.Domain.Entities;

namespace Merge.Infrastructure.Data;

/// <summary>
/// Primary ApplicationDbContext - Identity focused with assembly-wide configuration scanning.
/// BOLUM 1.1: Modular DbContext (ZORUNLU)
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, Role, Guid>(options), Merge.Application.Interfaces.IDbContext
{

    // Explicit implementation of IDbContext interface members
    DbSet<User> Merge.Application.Interfaces.IDbContext.Users => base.Users;
    DbSet<Role> Merge.Application.Interfaces.IDbContext.Roles => base.Roles;
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> Merge.Application.Interfaces.IDbContext.UserRoles => base.UserRoles;
    
    // Generic Set<TEntity> for repositories
    DbSet<TEntity> Merge.Application.Interfaces.IDbContext.Set<TEntity>() => base.Set<TEntity>();

    // Outbox messages for Domain Event dispatching (BOLUM 3.0)
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    // Audit logs for system tracking
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ✅ PERFORMANCE: Global Query Filter for Soft Delete (BOLUM 6.2)
        ConfigureGlobalQueryFilters(modelBuilder);

        // ✅ BOLUM 1.1: Discovery-based Configuration
        // Automatically find and apply all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                        System.Linq.Expressions.Expression.Constant(false)
                    ),
                    parameter
                );
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
