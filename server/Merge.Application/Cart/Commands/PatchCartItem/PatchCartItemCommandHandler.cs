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

namespace Merge.Application.Cart.Commands.PatchCartItem;

/// <summary>
/// Handler for PatchCartItemCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchCartItemCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<PatchCartItemCommandHandler> logger,
    IOptions<CartSettings> cartSettings) : IRequestHandler<PatchCartItemCommand, bool>
{
    public async Task<bool> Handle(PatchCartItemCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching cart item. CartItemId: {CartItemId}", request.CartItemId);

        if (!request.Quantity.HasValue)
        {
            logger.LogWarning("No fields to update for cart item {CartItemId}", request.CartItemId);
            return true; // Nothing to update
        }

        var cartItem = await context.Set<CartItem>()
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        if (cartItem == null)
        {
            logger.LogWarning("Cart item not found. CartItemId: {CartItemId}", request.CartItemId);
            return false;
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only product query
        var product = await context.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product {ProductId} not found for cart item {CartItemId}", cartItem.ProductId, request.CartItemId);
            throw new NotFoundException("Ürün", cartItem.ProductId);
        }

        if (product.StockQuantity < request.Quantity.Value)
        {
            logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                cartItem.ProductId, product.StockQuantity, request.Quantity.Value);
            throw new BusinessException("Yeterli stok yok.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Cart aggregate root üzerinden güncelleme
            var cart = await context.Set<Cart>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartItem.CartId, cancellationToken);

            if (cart == null)
            {
                logger.LogWarning("Cart not found for cart item {CartItemId}", request.CartItemId);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            var maxQuantity = cartSettings.Value.MaxCartItemQuantity;

            // ✅ BOLUM 1.1: Rich Domain Model - Cart entity method kullanımı
            cart.UpdateItemQuantity(request.CartItemId, request.Quantity.Value, maxQuantity);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Cart item patched successfully. CartItemId: {CartItemId}, NewQuantity: {Quantity}",
                request.CartItemId, request.Quantity.Value);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error patching cart item. CartItemId: {CartItemId}", request.CartItemId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
