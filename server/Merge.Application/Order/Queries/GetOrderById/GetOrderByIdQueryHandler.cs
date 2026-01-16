using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Ordering;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        return order == null ? null : _mapper.Map<OrderDto>(order);
    }
}
