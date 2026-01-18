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

public class GetSplitOrdersQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetSplitOrdersQuery, IEnumerable<OrderSplitDto>>
{

    public async Task<IEnumerable<OrderSplitDto>> Handle(GetSplitOrdersQuery request, CancellationToken cancellationToken)
    {
        var splits = await context.Set<OrderSplit>()
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

        return mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }
}
