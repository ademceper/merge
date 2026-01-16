using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Ordering;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

namespace Merge.Infrastructure.Data.Contexts;

public class PaymentDbContext : DbContext, IDbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Merge.Domain.Modules.Payment.Payment> Payments { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }
    public DbSet<GiftCardTransaction> GiftCardTransactions { get; set; }
    public DbSet<Merge.Domain.Modules.Payment.Currency> Currencies { get; set; }
    public DbSet<CreditTerm> CreditTerms { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionUsage> SubscriptionUsages { get; set; }
    public DbSet<FraudDetectionRule> FraudDetectionRules { get; set; }
    public DbSet<FraudAlert> FraudAlerts { get; set; }
    public DbSet<PaymentFraudPrevention> PaymentFraudPreventions { get; set; }
    public DbSet<TaxRule> TaxRules { get; set; }
    public DbSet<VolumeDiscount> VolumeDiscounts { get; set; }
    public DbSet<WholesalePrice> WholesalePrices { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    // ✅ LSP FIX: Anlamlı hata mesajı - ISP gerektirir
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users =>
        throw new InvalidOperationException("PaymentDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles =>
        throw new InvalidOperationException("PaymentDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles =>
        throw new InvalidOperationException("PaymentDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Payment");

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
