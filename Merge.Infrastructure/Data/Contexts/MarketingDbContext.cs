using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

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
    
    // Identity related sets are not here, but IDbContext requires them if we use it directly
    // This is why we eventually want specific interfaces per context
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users => throw new NotImplementedException();
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles => throw new NotImplementedException();
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketingDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Marketing");
    }
}
