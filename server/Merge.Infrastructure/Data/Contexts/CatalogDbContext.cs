using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using Merge.Infrastructure.Data.Configurations.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Infrastructure.Data.Contexts;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options), IDbContext
{

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<ProductBundle> ProductBundles { get; set; }
    public DbSet<BundleItem> BundleItems { get; set; }
    public DbSet<RecentlyViewedProduct> RecentlyViewedProducts { get; set; }
    public DbSet<SearchHistory> SearchHistories { get; set; }
    public DbSet<PopularSearch> PopularSearches { get; set; }
    public DbSet<SharedWishlist> SharedWishlists { get; set; }
    public DbSet<SharedWishlistItem> SharedWishlistItems { get; set; }
    public DbSet<ProductComparison> ProductComparisons { get; set; }
    public DbSet<ProductComparisonItem> ProductComparisonItems { get; set; }
    public DbSet<SizeGuide> SizeGuides { get; set; }
    public DbSet<SizeGuideEntry> SizeGuideEntries { get; set; }
    public DbSet<ProductSizeGuide> ProductSizeGuides { get; set; }
    public DbSet<VirtualTryOn> VirtualTryOns { get; set; }
    public DbSet<ProductQuestion> ProductQuestions { get; set; }
    public DbSet<ProductAnswer> ProductAnswers { get; set; }
    public DbSet<ReviewMedia> ReviewMedias { get; set; }
    public DbSet<ReviewHelpfulness> ReviewHelpfulnesses { get; set; }
    public DbSet<QuestionHelpfulness> QuestionHelpfulnesses { get; set; }
    public DbSet<AnswerHelpfulness> AnswerHelpfulnesses { get; set; }
    public DbSet<ProductTrustBadge> ProductTrustBadges { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    // IDbContext implementations for compatibility
    // ✅ LSP FIX: Anlamlı hata mesajı - ISP gerektirir (tutarlılık için InvalidOperationException kullanılıyor)
    DbSet<User> IDbContext.Users => throw new InvalidOperationException("CatalogDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Role> IDbContext.Roles => throw new InvalidOperationException("CatalogDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new InvalidOperationException("CatalogDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations for the Catalog module
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly, 
            type => type.Namespace?.Contains("Catalog") ?? false);

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
