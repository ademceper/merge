using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Cart.Commands.ClearRecentlyViewed;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ClearRecentlyViewedCommandHandler : IRequestHandler<ClearRecentlyViewedCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearRecentlyViewedCommandHandler> _logger;

    public ClearRecentlyViewedCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ClearRecentlyViewedCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ClearRecentlyViewedCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Bulk delete instead of foreach DeleteAsync (N+1 fix)
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
        var recentlyViewed = await _context.Set<RecentlyViewedProduct>()
            .Where(rvp => rvp.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var item in recentlyViewed)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
    }
}

