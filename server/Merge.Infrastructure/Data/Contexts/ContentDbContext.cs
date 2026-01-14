using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Content;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

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
    
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users => throw new NotImplementedException();
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles => throw new NotImplementedException();
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Content");
    }
}
