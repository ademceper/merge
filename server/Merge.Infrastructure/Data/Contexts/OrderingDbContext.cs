using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;
using Merge.Infrastructure.Data.Configurations.Ordering;

namespace Merge.Infrastructure.Data.Contexts;

public class OrderingDbContext : DbContext, IDbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderSplit> OrderSplits { get; set; }
    public DbSet<OrderSplitItem> OrderSplitItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<SavedCartItem> SavedCartItems { get; set; }
    public DbSet<Shipping> Shippings { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<PreOrder> PreOrders { get; set; }
    public DbSet<LiveStreamOrder> LiveStreamOrders { get; set; }
    public DbSet<OrderVerification> OrderVerifications { get; set; }
    public DbSet<InternationalShipping> InternationalShippings { get; set; }
    public DbSet<CustomsDeclaration> CustomsDeclarations { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    // ✅ LSP FIX: Anlamlı hata mesajı - ISP gerektirir (tutarlılık için InvalidOperationException kullanılıyor)
    DbSet<User> IDbContext.Users => throw new InvalidOperationException("OrderingDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Role> IDbContext.Roles => throw new InvalidOperationException("OrderingDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new InvalidOperationException("OrderingDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations for the Ordering module
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderConfiguration).Assembly, 
            type => type.Namespace?.Contains("Ordering") ?? false);

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
