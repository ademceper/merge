using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Catalog;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.AddToWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddToWishlistCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AddToWishlistCommandHandler> logger) : IRequestHandler<AddToWishlistCommand, bool>
{

    public async Task<bool> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}",
            request.ProductId, request.UserId);

        // ✅ PERFORMANCE: AsNoTracking for read-only check
        // ✅ PERFORMANCE: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        var existing = await context.Set<Wishlist>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (existing is not null)
        {
            logger.LogWarning(
                "Product {ProductId} already exists in wishlist for user {UserId}",
                request.ProductId, request.UserId);
            return false; // Zaten favorilerde
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only product query
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (product is null || !product.IsActive)
        {
            logger.LogWarning(
                "Product {ProductId} not found or inactive for user {UserId}",
                request.ProductId, request.UserId);
            throw new NotFoundException("Ürün", request.ProductId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        var wishlist = Wishlist.Create(request.UserId, request.ProductId);

        await context.Set<Wishlist>().AddAsync(wishlist, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully added product {ProductId} to wishlist for user {UserId}",
            request.ProductId, request.UserId);

        return true;
    }
}

