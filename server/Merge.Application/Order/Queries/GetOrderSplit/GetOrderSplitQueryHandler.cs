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

namespace Merge.Application.Order.Queries.GetOrderSplit;

public class GetOrderSplitQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetOrderSplitQuery, OrderSplitDto?>
{

    public async Task<OrderSplitDto?> Handle(GetOrderSplitQuery request, CancellationToken cancellationToken)
    {
        var split = await context.Set<OrderSplit>()
            .AsNoTracking()
            .Include(s => s.OriginalOrder)
            .Include(s => s.SplitOrder)
            .Include(s => s.NewAddress)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.OriginalOrderItem)
                    .ThenInclude(oi => oi.Product)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.SplitOrderItem)
            .FirstOrDefaultAsync(s => s.Id == request.SplitId, cancellationToken);

        return split is not null ? mapper.Map<OrderSplitDto>(split) : null;
    }
}
