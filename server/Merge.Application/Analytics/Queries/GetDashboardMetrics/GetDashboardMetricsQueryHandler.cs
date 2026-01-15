using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetDashboardMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetDashboardMetricsQueryHandler(
    IDbContext context,
    ILogger<GetDashboardMetricsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IMapper mapper) : IRequestHandler<GetDashboardMetricsQuery, List<DashboardMetricDto>>
{

    public async Task<List<DashboardMetricDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching dashboard metrics. Category: {Category}", request.Category);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !m.IsDeleted check (Global Query Filter handles it)
        var query = context.Set<DashboardMetric>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(m => m.Category == request.Category);
        }

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var metrics = await query
            .OrderByDescending(m => m.CalculatedAt)
            .Take(settings.Value.MaxQueryLimit)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return mapper.Map<List<DashboardMetricDto>>(metrics);
    }
}

