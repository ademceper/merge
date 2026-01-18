using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveCartItemCommandHandler> logger) : IRequestHandler<RemoveCartItemCommand, bool>
{

    public async Task<bool> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing item from cart. CartItemId: {CartItemId}", request.CartItemId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var cartItem = await context.Set<CartItem>()
                .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);
            
            if (cartItem is null)
            {
                logger.LogWarning("Cart item {CartItemId} not found", request.CartItemId);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            // Cart entity'sinin RemoveItem method'unu kullan
            var cart = await context.Set<CartEntity>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartItem.CartId, cancellationToken);

            if (cart is null)
            {
                logger.LogWarning("Cart not found for cart item {CartItemId}", request.CartItemId);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            cart.RemoveItem(request.CartItemId);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Successfully removed item from cart. CartItemId: {CartItemId}, ProductId: {ProductId}",
                request.CartItemId, cartItem.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error removing item from cart. CartItemId: {CartItemId}",
                request.CartItemId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

