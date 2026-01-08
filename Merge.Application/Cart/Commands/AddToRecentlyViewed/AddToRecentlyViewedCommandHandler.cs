using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Cart.Commands.AddToRecentlyViewed;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddToRecentlyViewedCommandHandler : IRequestHandler<AddToRecentlyViewedCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToRecentlyViewedCommandHandler> _logger;
    private readonly CartSettings _cartSettings;

    public AddToRecentlyViewedCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AddToRecentlyViewedCommandHandler> logger,
        IOptions<CartSettings> cartSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cartSettings = cartSettings.Value;
    }

    public async Task Handle(AddToRecentlyViewedCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
        var existing = await _context.Set<RecentlyViewedProduct>()
            .FirstOrDefaultAsync(rvp => rvp.UserId == request.UserId &&
                                      rvp.ProductId == request.ProductId, cancellationToken);

        if (existing != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            existing.UpdateViewedAt();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
            var recentlyViewed = RecentlyViewedProduct.Create(request.UserId, request.ProductId);

            await _context.Set<RecentlyViewedProduct>().AddAsync(recentlyViewed, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
            // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
            var count = await _context.Set<RecentlyViewedProduct>()
                .CountAsync(rvp => rvp.UserId == request.UserId, cancellationToken);

            // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
            if (count > _cartSettings.MaxRecentlyViewedItems)
            {
                // ✅ PERFORMANCE: Bulk delete instead of foreach DeleteAsync (N+1 fix)
                // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
                var oldest = await _context.Set<RecentlyViewedProduct>()
                    .Where(rvp => rvp.UserId == request.UserId)
                    .OrderBy(rvp => rvp.ViewedAt)
                    .Take(count - _cartSettings.MaxRecentlyViewedItems)
                    .ToListAsync(cancellationToken);

                foreach (var item in oldest)
                {
                    // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
                    item.MarkAsDeleted();
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
            }
        }
    }
}

