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
    
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users => throw new NotImplementedException();
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles => throw new NotImplementedException();
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Analytics");
    }
}
