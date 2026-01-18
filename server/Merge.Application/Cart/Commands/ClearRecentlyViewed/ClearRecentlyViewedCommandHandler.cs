using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.ClearRecentlyViewed;

public class ClearRecentlyViewedCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ClearRecentlyViewedCommandHandler> logger) : IRequestHandler<ClearRecentlyViewedCommand>
{

    public async Task Handle(ClearRecentlyViewedCommand request, CancellationToken cancellationToken)
    {
        var recentlyViewed = await context.Set<RecentlyViewedProduct>()
            .Where(rvp => rvp.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var item in recentlyViewed)
        {
            item.MarkAsDeleted();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
    }
}

