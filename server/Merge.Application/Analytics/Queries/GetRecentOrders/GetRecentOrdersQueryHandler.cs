using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Order;
using Merge.Application.Configuration;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using AutoMapper;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Analytics.Queries.GetRecentOrders;

public class GetRecentOrdersQueryHandler(
    IDbContext context,
    ILogger<GetRecentOrdersQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IMapper mapper) : IRequestHandler<GetRecentOrdersQuery, IEnumerable<OrderDto>>
{

    public async Task<IEnumerable<OrderDto>> Handle(GetRecentOrdersQuery request, CancellationToken cancellationToken)
    {
        var count = request.Count == 10 ? settings.Value.TopProductsLimit : request.Count; 
        
        var orders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}

