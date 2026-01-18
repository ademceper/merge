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

namespace Merge.Application.Cart.Queries.IsInWishlist;

public class IsInWishlistQueryHandler(
    IDbContext context,
    ILogger<IsInWishlistQueryHandler> logger) : IRequestHandler<IsInWishlistQuery, bool>
{

    public async Task<bool> Handle(IsInWishlistQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking if product {ProductId} is in wishlist for user {UserId}",
            request.ProductId, request.UserId);

        var wishlistProductIds = await context.Set<Wishlist>()
            .AsNoTracking()
            .Where(w => w.UserId == request.UserId)
            .Select(w => w.ProductId)
            .ToListAsync(cancellationToken);

        var exists = wishlistProductIds.Contains(request.ProductId);

        logger.LogDebug("Product {ProductId} exists in wishlist for user {UserId}: {Exists}",
            request.ProductId, request.UserId, exists);

        return exists;
    }
}

