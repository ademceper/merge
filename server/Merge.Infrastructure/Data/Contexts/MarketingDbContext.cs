using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;
using User = Merge.Domain.Modules.Identity.User;
using Role = Merge.Domain.Modules.Identity.Role;

namespace Merge.Infrastructure.Data.Contexts;

public class MarketingDbContext : DbContext, IDbContext
{
    public MarketingDbContext(DbContextOptions<MarketingDbContext> options) : base(options)
    {
    }

    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<FlashSale> FlashSales { get; set; }
    public DbSet<FlashSaleProduct> FlashSaleProducts { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<AbandonedCartEmail> AbandonedCartEmails { get; set; }
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    public DbSet<LoyaltyTier> LoyaltyTiers { get; set; }
    public DbSet<LoyaltyRule> LoyaltyRules { get; set; }
    public DbSet<ReferralCode> ReferralCodes { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    public DbSet<PreOrderCampaign> PreOrderCampaigns { get; set; }
    public DbSet<EmailCampaign> EmailCampaigns { get; set; }
    public DbSet<EmailCampaignRecipient> EmailCampaignRecipients { get; set; }
    public DbSet<EmailSubscriber> EmailSubscribers { get; set; }
    public DbSet<LiveStream> LiveStreams { get; set; }
    public DbSet<LiveStreamProduct> LiveStreamProducts { get; set; }
    public DbSet<LiveStreamViewer> LiveStreamViewers { get; set; }

    // IDbContext explicit implementations (if any missing from base DbContext)
    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    // ✅ LSP FIX: Anlamlı hata mesajı - ISP gerektirir
    DbSet<User> IDbContext.Users =>
        throw new InvalidOperationException("MarketingDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Role> IDbContext.Roles =>
        throw new InvalidOperationException("MarketingDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles =>
        throw new InvalidOperationException("MarketingDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketingDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Marketing");

        // ✅ HIGH-DB-003 FIX: Global Query Filter - Soft Delete (ZORUNLU)
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
