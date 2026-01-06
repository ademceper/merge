using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.Analytics.Queries.GetDashboardMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, List<DashboardMetricDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetDashboardMetricsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly IMapper _mapper;

    public GetDashboardMetricsQueryHandler(
        IDbContext context,
        ILogger<GetDashboardMetricsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
        _mapper = mapper;
    }

    public async Task<List<DashboardMetricDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard metrics. Category: {Category}", request.Category);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !m.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<DashboardMetric>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(m => m.Category == request.Category);
        }

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var metrics = await query
            .OrderByDescending(m => m.CalculatedAt)
            .Take(_settings.MaxQueryLimit)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<List<DashboardMetricDto>>(metrics);
    }
}

