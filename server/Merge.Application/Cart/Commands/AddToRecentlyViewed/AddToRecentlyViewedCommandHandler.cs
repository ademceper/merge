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

public class AddToRecentlyViewedCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AddToRecentlyViewedCommandHandler> logger,
    IOptions<CartSettings> cartSettings) : IRequestHandler<AddToRecentlyViewedCommand>
{

    public async Task Handle(AddToRecentlyViewedCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.Set<RecentlyViewedProduct>()
            .FirstOrDefaultAsync(rvp => rvp.UserId == request.UserId &&
                                      rvp.ProductId == request.ProductId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateViewedAt();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var recentlyViewed = RecentlyViewedProduct.Create(request.UserId, request.ProductId);

            await context.Set<RecentlyViewedProduct>().AddAsync(recentlyViewed, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var count = await context.Set<RecentlyViewedProduct>()
                .CountAsync(rvp => rvp.UserId == request.UserId, cancellationToken);

            if (count > cartSettings.Value.MaxRecentlyViewedItems)
            {
                var oldest = await context.Set<RecentlyViewedProduct>()
                    .Where(rvp => rvp.UserId == request.UserId)
                    .OrderBy(rvp => rvp.ViewedAt)
                    .Take(count - cartSettings.Value.MaxRecentlyViewedItems)
                    .ToListAsync(cancellationToken);

                foreach (var item in oldest)
                {
                    item.MarkAsDeleted();
                }

                await unitOfWork.SaveChangesAsync(cancellationToken); 
            }
        }
    }
}

