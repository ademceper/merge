using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Ordering;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Order.Queries.GetOrdersByUserId;

public class GetOrdersByUserIdQueryHandler(
    IDbContext context,
    IMapper mapper) : IRequestHandler<GetOrdersByUserIdQuery, IEnumerable<OrderDto>>
{

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var orders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}
