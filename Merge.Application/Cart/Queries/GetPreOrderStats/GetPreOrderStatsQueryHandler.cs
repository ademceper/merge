using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;

namespace Merge.Application.Cart.Queries.GetPreOrderStats;

public class GetPreOrderStatsQueryHandler : IRequestHandler<GetPreOrderStatsQuery, PreOrderStatsDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetPreOrderStatsQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PreOrderStatsDto> Handle(GetPreOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var totalPreOrders = await _context.Set<PreOrder>()
            .CountAsync(cancellationToken);

        var pendingPreOrders = await _context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Pending, cancellationToken);

        var confirmedPreOrders = await _context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Confirmed || po.Status == PreOrderStatus.DepositPaid, cancellationToken);

        var totalRevenue = await _context.Set<PreOrder>()
            .SumAsync(po => po.Price * po.Quantity, cancellationToken);

        var totalDeposits = await _context.Set<PreOrder>()
            .SumAsync(po => po.DepositPaid, cancellationToken);

        var recentPreOrders = await _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .OrderByDescending(po => po.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var recentDtos = _mapper.Map<IEnumerable<PreOrderDto>>(recentPreOrders).ToList();

        return new PreOrderStatsDto
        {
            TotalPreOrders = totalPreOrders,
            PendingPreOrders = pendingPreOrders,
            ConfirmedPreOrders = confirmedPreOrders,
            TotalRevenue = totalRevenue,
            TotalDeposits = totalDeposits,
            RecentPreOrders = recentDtos
        };
    }
}

