using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;
using User = Merge.Domain.Modules.Identity.User;
using Role = Merge.Domain.Modules.Identity.Role;

namespace Merge.Infrastructure.Data.Contexts;

public class ContentDbContext : DbContext, IDbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    public DbSet<Banner> Banners { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<StaticTranslation> StaticTranslations { get; set; }
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
    public DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories { get; set; }
    public DbSet<KnowledgeBaseView> KnowledgeBaseViews { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyAcceptance> PolicyAcceptances { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogComment> BlogComments { get; set; }
    public DbSet<BlogPostView> BlogPostViews { get; set; }
    public DbSet<SEOSettings> SEOSettings { get; set; }
    public DbSet<SitemapEntry> SitemapEntries { get; set; }
    public DbSet<CMSPage> CMSPages { get; set; }
    public DbSet<LandingPage> LandingPages { get; set; }
    public DbSet<PageBuilder> PageBuilders { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    // ✅ LSP FIX: Anlamlı hata mesajı - ISP gerektirir
    DbSet<User> IDbContext.Users =>
        throw new InvalidOperationException("ContentDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Role> IDbContext.Roles =>
        throw new InvalidOperationException("ContentDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles =>
        throw new InvalidOperationException("ContentDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Content");

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
