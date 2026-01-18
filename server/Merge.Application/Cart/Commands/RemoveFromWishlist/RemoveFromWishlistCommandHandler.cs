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

namespace Merge.Application.Cart.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveFromWishlistCommandHandler> logger) : IRequestHandler<RemoveFromWishlistCommand, bool>
{

    public async Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}",
            request.ProductId, request.UserId);

        var wishlist = await context.Set<Wishlist>()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        if (wishlist is null)
        {
            logger.LogWarning(
                "Wishlist item not found for product {ProductId} and user {UserId}",
                request.ProductId, request.UserId);
            return false;
        }

        wishlist.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully removed product {ProductId} from wishlist for user {UserId}",
            request.ProductId, request.UserId);

        return true;
    }
}

