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
using Product = Merge.Domain.Modules.Catalog.Product;
using Cart = Merge.Domain.Modules.Ordering.Cart;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.UpdateCartItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateCartItemCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCartItemCommandHandler> logger,
    IOptions<CartSettings> cartSettings) : IRequestHandler<UpdateCartItemCommand, bool>
{

    public async Task<bool> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Updating cart item quantity. CartItemId: {CartItemId}, NewQuantity: {Quantity}",
            request.CartItemId, request.Quantity);

        var cartItem = await context.Set<CartItem>()
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (cartItem is null)
        {
            logger.LogWarning("Cart item {CartItemId} not found", request.CartItemId);
            return false;
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only product query
        var product = await context.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (product is null)
        {
            logger.LogWarning(
                "Product {ProductId} not found for cart item {CartItemId}",
                cartItem.ProductId, request.CartItemId);
            throw new NotFoundException("Ürün", cartItem.ProductId);
        }

        if (product.StockQuantity < request.Quantity)
        {
            logger.LogWarning(
                "Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                cartItem.ProductId, product.StockQuantity, request.Quantity);
            throw new BusinessException("Yeterli stok yok.");
        }

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Cart + CartItem + Updates)
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Cart entity üzerinden domain method kullanımı
            // Cart aggregate root olduğu için, CartItem güncellemeleri Cart üzerinden yapılmalı
            var cart = await context.Set<Cart>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartItem.CartId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (cart is null)
            {
                logger.LogWarning("Cart not found for cart item {CartItemId}", request.CartItemId);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration'dan al
            var maxQuantity = cartSettings.Value.MaxCartItemQuantity;

            // ✅ BOLUM 1.1: Rich Domain Model - Cart entity method kullanımı
            // ✅ ARCHITECTURE: Domain event'ler Cart.UpdateItemQuantity() içinde otomatik olarak oluşturulur
            cart.UpdateItemQuantity(request.CartItemId, request.Quantity, maxQuantity);
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Successfully updated cart item quantity. CartItemId: {CartItemId}, NewQuantity: {Quantity}, ProductId: {ProductId}",
                request.CartItemId, request.Quantity, cartItem.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error updating cart item quantity. CartItemId: {CartItemId}, Quantity: {Quantity}",
                request.CartItemId, request.Quantity);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

