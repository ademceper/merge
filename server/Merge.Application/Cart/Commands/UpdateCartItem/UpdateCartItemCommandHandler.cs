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
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.UpdateCartItem;

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
        
        if (cartItem is null)
        {
            logger.LogWarning("Cart item {CartItemId} not found", request.CartItemId);
            return false;
        }

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId, cancellationToken);
        
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

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Cart aggregate root olduğu için, CartItem güncellemeleri Cart üzerinden yapılmalı
            var cart = await context.Set<CartEntity>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartItem.CartId, cancellationToken);

            if (cart is null)
            {
                logger.LogWarning("Cart not found for cart item {CartItemId}", request.CartItemId);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            var maxQuantity = cartSettings.Value.MaxCartItemQuantity;

            cart.UpdateItemQuantity(request.CartItemId, request.Quantity, maxQuantity);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Successfully updated cart item quantity. CartItemId: {CartItemId}, NewQuantity: {Quantity}, ProductId: {ProductId}",
                request.CartItemId, request.Quantity, cartItem.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error updating cart item quantity. CartItemId: {CartItemId}, Quantity: {Quantity}",
                request.CartItemId, request.Quantity);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

