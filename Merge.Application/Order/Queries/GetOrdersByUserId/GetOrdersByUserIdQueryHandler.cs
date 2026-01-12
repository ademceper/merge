using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Order.Queries.GetOrdersByUserId;

public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, IEnumerable<OrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersByUserIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var orders = await _context.Set<Merge.Domain.Modules.Ordering.Order>()
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}
