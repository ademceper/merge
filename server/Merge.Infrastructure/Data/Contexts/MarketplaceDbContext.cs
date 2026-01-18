using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;
using User = Merge.Domain.Modules.Identity.User;
using Role = Merge.Domain.Modules.Identity.Role;

namespace Merge.Infrastructure.Data.Contexts;

public class MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : DbContext(options), IDbContext
{

    public DbSet<SellerProfile> SellerProfiles { get; set; }
    public DbSet<SellerApplication> SellerApplications { get; set; }
    public DbSet<SellerDocument> SellerDocuments { get; set; }
    public DbSet<SellerCommission> SellerCommissions { get; set; }
    public DbSet<SellerCommissionSettings> SellerCommissionSettings { get; set; }
    public DbSet<CommissionPayout> CommissionPayouts { get; set; }
    public DbSet<CommissionPayoutItem> CommissionPayoutItems { get; set; }
    public DbSet<TrustBadge> TrustBadges { get; set; }
    public DbSet<SellerTrustBadge> SellerTrustBadges { get; set; }
    public DbSet<SellerTransaction> SellerTransactions { get; set; }
    public DbSet<SellerInvoice> SellerInvoices { get; set; }
    public DbSet<Store> Stores { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    DbSet<User> IDbContext.Users =>
        throw new InvalidOperationException("MarketplaceDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Role> IDbContext.Roles =>
        throw new InvalidOperationException("MarketplaceDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles =>
        throw new InvalidOperationException("MarketplaceDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketplaceDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Marketplace");

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
