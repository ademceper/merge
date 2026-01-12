using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.AddToWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToWishlistCommandHandler> _logger;

    public AddToWishlistCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AddToWishlistCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}",
            request.ProductId, request.UserId);

        // ✅ PERFORMANCE: AsNoTracking for read-only check
        // ✅ PERFORMANCE: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        var existing = await _context.Set<Wishlist>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.ProductId == request.ProductId, cancellationToken);

        if (existing != null)
        {
            _logger.LogWarning(
                "Product {ProductId} already exists in wishlist for user {UserId}",
                request.ProductId, request.UserId);
            return false; // Zaten favorilerde
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only product query
        var product = await _context.Set<Merge.Domain.Modules.Catalog.Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        
        if (product == null || !product.IsActive)
        {
            _logger.LogWarning(
                "Product {ProductId} not found or inactive for user {UserId}",
                request.ProductId, request.UserId);
            throw new NotFoundException("Ürün", request.ProductId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        var wishlist = Wishlist.Create(request.UserId, request.ProductId);

        await _context.Set<Wishlist>().AddAsync(wishlist, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully added product {ProductId} to wishlist for user {UserId}",
            request.ProductId, request.UserId);

        return true;
    }
}

