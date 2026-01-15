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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RemoveFromWishlistCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveFromWishlistCommandHandler> logger) : IRequestHandler<RemoveFromWishlistCommand, bool>
{

    public async Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}",
            request.ProductId, request.UserId);

        // ✅ PERFORMANCE: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        var wishlist = await context.Set<Wishlist>()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (wishlist is null)
        {
            logger.LogWarning(
                "Wishlist item not found for product {ProductId} and user {UserId}",
                request.ProductId, request.UserId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        wishlist.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully removed product {ProductId} from wishlist for user {UserId}",
            request.ProductId, request.UserId);

        return true;
    }
}

