using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;
using Merge.Domain.Entities;

namespace Merge.Infrastructure.Data.Contexts;

public class AnalyticsDbContext : DbContext, IDbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    public DbSet<ExchangeRateHistory> ExchangeRateHistories { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<DashboardMetric> DashboardMetrics { get; set; }
    public DbSet<DataWarehouse> DataWarehouses { get; set; }
    public DbSet<ETLProcess> ETLProcesses { get; set; }
    public DbSet<DataPipeline> DataPipelines { get; set; }
    public DbSet<DataQualityRule> DataQualityRules { get; set; }
    public DbSet<DataQualityCheck> DataQualityChecks { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    // ✅ LSP FIX: Anlamlı hata mesajı - ISP gerektirir
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users =>
        throw new InvalidOperationException("AnalyticsDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles =>
        throw new InvalidOperationException("AnalyticsDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles =>
        throw new InvalidOperationException("AnalyticsDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Analytics");

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
