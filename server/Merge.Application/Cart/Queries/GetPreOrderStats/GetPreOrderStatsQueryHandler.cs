using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetPreOrderStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPreOrderStatsQueryHandler(
    IDbContext context,
    IMapper mapper) : IRequestHandler<GetPreOrderStatsQuery, PreOrderStatsDto>
{

    public async Task<PreOrderStatsDto> Handle(GetPreOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var totalPreOrders = await context.Set<PreOrder>()
            .CountAsync(cancellationToken);

        var pendingPreOrders = await context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Pending, cancellationToken);

        var confirmedPreOrders = await context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Confirmed || po.Status == PreOrderStatus.DepositPaid, cancellationToken);

        var totalRevenue = await context.Set<PreOrder>()
            .SumAsync(po => po.Price * po.Quantity, cancellationToken);

        var totalDeposits = await context.Set<PreOrder>()
            .SumAsync(po => po.DepositPaid, cancellationToken);

        var recentPreOrders = await context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .OrderByDescending(po => po.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var recentDtos = mapper.Map<IEnumerable<PreOrderDto>>(recentPreOrders).ToList();

        return new PreOrderStatsDto(
            totalPreOrders,
            pendingPreOrders,
            confirmedPreOrders,
            totalRevenue,
            totalDeposits,
            recentDtos
        );
    }
}

