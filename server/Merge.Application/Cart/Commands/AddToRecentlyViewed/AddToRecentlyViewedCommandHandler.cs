using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.AddToRecentlyViewed;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddToRecentlyViewedCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AddToRecentlyViewedCommandHandler> logger,
    IOptions<CartSettings> cartSettings) : IRequestHandler<AddToRecentlyViewedCommand>
{

    public async Task Handle(AddToRecentlyViewedCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
        var existing = await context.Set<RecentlyViewedProduct>()
            .FirstOrDefaultAsync(rvp => rvp.UserId == request.UserId &&
                                      rvp.ProductId == request.ProductId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (existing is not null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            existing.UpdateViewedAt();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
            var recentlyViewed = RecentlyViewedProduct.Create(request.UserId, request.ProductId);

            await context.Set<RecentlyViewedProduct>().AddAsync(recentlyViewed, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
            // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
            var count = await context.Set<RecentlyViewedProduct>()
                .CountAsync(rvp => rvp.UserId == request.UserId, cancellationToken);

            // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
            if (count > cartSettings.Value.MaxRecentlyViewedItems)
            {
                // ✅ PERFORMANCE: Bulk delete instead of foreach DeleteAsync (N+1 fix)
                // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
                var oldest = await context.Set<RecentlyViewedProduct>()
                    .Where(rvp => rvp.UserId == request.UserId)
                    .OrderBy(rvp => rvp.ViewedAt)
                    .Take(count - cartSettings.Value.MaxRecentlyViewedItems)
                    .ToListAsync(cancellationToken);

                foreach (var item in oldest)
                {
                    // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
                    item.MarkAsDeleted();
                }

                await unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
            }
        }
    }
}

