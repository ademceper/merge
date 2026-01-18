using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Ordering;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(
    IDbContext context,
    IMapper mapper) : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        return order is null ? null : mapper.Map<OrderDto>(order);
    }
}
