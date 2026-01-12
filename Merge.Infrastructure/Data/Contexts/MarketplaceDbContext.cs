using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Marketplace;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

namespace Merge.Infrastructure.Data.Contexts;

public class MarketplaceDbContext : DbContext, IDbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options)
    {
    }

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
    
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users => throw new NotImplementedException();
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles => throw new NotImplementedException();
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketplaceDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Marketplace");
    }
}
