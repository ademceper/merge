using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetRecoveryStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetRecoveryStatsQueryHandler : IRequestHandler<GetRecoveryStatsQuery, AbandonedCartRecoveryStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetRecoveryStatsQueryHandler> _logger;

    public GetRecoveryStatsQueryHandler(
        IDbContext context,
        ILogger<GetRecoveryStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AbandonedCartRecoveryStatsDto> Handle(GetRecoveryStatsQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-request.Days);
        var minDate = DateTime.UtcNow.AddDays(-request.Days);
        var maxDate = DateTime.UtcNow.AddHours(-1);

        // ✅ PERFORMANCE: Database'de Count ve Sum yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var abandonedCartIds = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
            .AsNoTracking()
            .Where(c => c.CartItems.Any() &&
                       c.UpdatedAt >= minDate &&
                       c.UpdatedAt <= maxDate)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        // Filter out carts that have been converted to orders
        var abandonedCartUserIds = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id))
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var userIdsWithOrders = await _context.Set<Merge.Domain.Modules.Ordering.Order>()
            .AsNoTracking()
            .Where(o => abandonedCartUserIds.Contains(o.UserId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var finalAbandonedCartIds = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id) && 
                       !userIdsWithOrders.Contains(c.UserId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var totalAbandonedCarts = finalAbandonedCartIds.Count;

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalAbandonedValue = await _context.Set<CartItem>()
            .AsNoTracking()
            .Where(ci => finalAbandonedCartIds.Contains(ci.CartId))
            .SumAsync(ci => ci.Price * ci.Quantity, cancellationToken);

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emailsSent = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate)
            .CountAsync(cancellationToken);

        var emailsOpened = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.WasOpened)
            .CountAsync(cancellationToken);

        var emailsClicked = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.WasClicked)
            .CountAsync(cancellationToken);

        var recoveredCarts = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.ResultedInPurchase)
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var recoveredRevenue = await _context.Set<Merge.Domain.Modules.Ordering.Order>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= startDate)
            .Join(
                _context.Set<AbandonedCartEmail>().AsNoTracking().Where(e => e.ResultedInPurchase),
                order => order.UserId,
                email => email.UserId,
                (order, email) => order.TotalAmount
            )
            .SumAsync(cancellationToken);

        return new AbandonedCartRecoveryStatsDto(
            totalAbandonedCarts,
            totalAbandonedValue,
            emailsSent,
            emailsOpened,
            emailsClicked,
            recoveredCarts,
            recoveredRevenue,
            totalAbandonedCarts > 0 ? (decimal)recoveredCarts / totalAbandonedCarts * 100 : 0,
            totalAbandonedCarts > 0 ? totalAbandonedValue / totalAbandonedCarts : 0
        );
    }
}

