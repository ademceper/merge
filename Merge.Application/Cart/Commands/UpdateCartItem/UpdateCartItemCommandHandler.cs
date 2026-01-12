using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.UpdateCartItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCartItemCommandHandler> _logger;
    private readonly CartSettings _cartSettings;

    public UpdateCartItemCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCartItemCommandHandler> logger,
        IOptions<CartSettings> cartSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cartSettings = cartSettings.Value;
    }

    public async Task<bool> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating cart item quantity. CartItemId: {CartItemId}, NewQuantity: {Quantity}",
            request.CartItemId, request.Quantity);

        var cartItem = await _context.Set<CartItem>()
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (cartItem is null)
        {
            _logger.LogWarning("Cart item {CartItemId} not found", request.CartItemId);
            return false;
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only product query
        var product = await _context.Set<Merge.Domain.Modules.Catalog.Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (product is null)
        {
            _logger.LogWarning(
                "Product {ProductId} not found for cart item {CartItemId}",
                cartItem.ProductId, request.CartItemId);
            throw new NotFoundException("Ürün", cartItem.ProductId);
        }

        if (product.StockQuantity < request.Quantity)
        {
            _logger.LogWarning(
                "Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                cartItem.ProductId, product.StockQuantity, request.Quantity);
            throw new BusinessException("Yeterli stok yok.");
        }

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration'dan al
        var maxQuantity = _cartSettings.MaxCartItemQuantity;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        cartItem.UpdateQuantity(request.Quantity, maxQuantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully updated cart item quantity. CartItemId: {CartItemId}, NewQuantity: {Quantity}, ProductId: {ProductId}",
            request.CartItemId, request.Quantity, cartItem.ProductId);

        return true;
    }
}

