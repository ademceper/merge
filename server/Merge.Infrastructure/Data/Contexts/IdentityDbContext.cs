using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

namespace Merge.Infrastructure.Data.Contexts;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : IdentityDbContext<User, Role, Guid>(options), IDbContext
{

    public DbSet<Address> Addresses { get; set; }
    public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; }
    public DbSet<TwoFactorCode> TwoFactorCodes { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<UserCurrencyPreference> UserCurrencyPreferences { get; set; }
    public DbSet<UserLanguagePreference> UserLanguagePreferences { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<OAuthProvider> OAuthProviders { get; set; }
    public DbSet<OAuthAccount> OAuthAccounts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AccountSecurityEvent> AccountSecurityEvents { get; set; }
    public DbSet<SecurityAlert> SecurityAlerts { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    // RBAC entities
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<StoreRole> StoreRoles { get; set; }
    public DbSet<OrganizationRole> OrganizationRoles { get; set; }
    public DbSet<StoreCustomerRole> StoreCustomerRoles { get; set; }

    // IDbContext implementations
    DbSet<User> IDbContext.Users => base.Users;
    DbSet<Role> IDbContext.Roles => base.Roles;
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => base.UserRoles;
    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations for the Identity module
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly, 
            type => type.Namespace?.Contains("Identity") ?? false);

        // Apply global filters
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
