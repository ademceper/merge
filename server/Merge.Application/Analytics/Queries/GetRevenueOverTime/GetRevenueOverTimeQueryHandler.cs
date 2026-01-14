using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetRevenueOverTime;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetRevenueOverTimeQueryHandler : IRequestHandler<GetRevenueOverTimeQuery, List<TimeSeriesDataPoint>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetRevenueOverTimeQueryHandler> _logger;

    public GetRevenueOverTimeQueryHandler(
        IDbContext context,
        ILogger<GetRevenueOverTimeQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TimeSeriesDataPoint>> Handle(GetRevenueOverTimeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching revenue over time. StartDate: {StartDate}, EndDate: {EndDate}, Interval: {Interval}",
            request.StartDate, request.EndDate, request.Interval);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        return await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate)
            .GroupBy(o => o.CreatedAt.Date)
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
            .Select(g => new TimeSeriesDataPoint(
                g.Key,
                g.Sum(o => o.TotalAmount),
                null, // Label
                g.Count()
            ))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);
    }
}

