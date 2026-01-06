using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Cart.Commands.RemoveFromWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFromWishlistCommandHandler> _logger;

    public RemoveFromWishlistCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RemoveFromWishlistCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}",
            request.ProductId, request.UserId);

        // ✅ PERFORMANCE: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        var wishlist = await _context.Set<Wishlist>()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        if (wishlist == null)
        {
            _logger.LogWarning(
                "Wishlist item not found for product {ProductId} and user {UserId}",
                request.ProductId, request.UserId);
            return false;
        }

        // Soft delete
        wishlist.IsDeleted = true;
        wishlist.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully removed product {ProductId} from wishlist for user {UserId}",
            request.ProductId, request.UserId);

        return true;
    }
}

