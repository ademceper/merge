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

public class GetRevenueOverTimeQueryHandler(
    IDbContext context,
    ILogger<GetRevenueOverTimeQueryHandler> logger) : IRequestHandler<GetRevenueOverTimeQuery, List<TimeSeriesDataPoint>>
{

    public async Task<List<TimeSeriesDataPoint>> Handle(GetRevenueOverTimeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching revenue over time. StartDate: {StartDate}, EndDate: {EndDate}, Interval: {Interval}",
            request.StartDate, request.EndDate, request.Interval);

        return await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate)
            .GroupBy(o => o.CreatedAt.Date)
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

