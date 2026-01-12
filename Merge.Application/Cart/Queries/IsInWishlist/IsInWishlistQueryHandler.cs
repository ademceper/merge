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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class IsInWishlistQueryHandler : IRequestHandler<IsInWishlistQuery, bool>
{
    private readonly IDbContext _context;
    private readonly ILogger<IsInWishlistQueryHandler> _logger;

    public IsInWishlistQueryHandler(
        IDbContext context,
        ILogger<IsInWishlistQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(IsInWishlistQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking if product {ProductId} is in wishlist for user {UserId}",
            request.ProductId, request.UserId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        // ✅ PERFORMANCE: Pre-fetch IDs and use Contains() instead of .Any() for better performance
        var wishlistProductIds = await _context.Set<Wishlist>()
            .AsNoTracking()
            .Where(w => w.UserId == request.UserId)
            .Select(w => w.ProductId)
            .ToListAsync(cancellationToken);

        var exists = wishlistProductIds.Contains(request.ProductId);

        _logger.LogDebug("Product {ProductId} exists in wishlist for user {UserId}: {Exists}",
            request.ProductId, request.UserId, exists);

        return exists;
    }
}

