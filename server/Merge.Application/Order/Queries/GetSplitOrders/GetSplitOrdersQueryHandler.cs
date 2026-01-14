using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Queries.GetSplitOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSplitOrdersQueryHandler : IRequestHandler<GetSplitOrdersQuery, IEnumerable<OrderSplitDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetSplitOrdersQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderSplitDto>> Handle(GetSplitOrdersQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var splits = await _context.Set<OrderSplit>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.OriginalOrder)
            .Include(s => s.SplitOrder)
            .Include(s => s.NewAddress)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.OriginalOrderItem)
                    .ThenInclude(oi => oi.Product)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.SplitOrderItem)
            .Where(s => s.SplitOrderId == request.SplitOrderId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }
}
