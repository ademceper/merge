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

namespace Merge.Application.Cart.Commands.ClearCart;

public class ClearCartCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ClearCartCommandHandler> logger) : IRequestHandler<ClearCartCommand, bool>
{

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var cart = await context.Set<CartEntity>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart is null)
            {
                logger.LogWarning("Cart not found for user {UserId}", request.UserId);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            var itemCount = cart.CartItems.Count(ci => !ci.IsDeleted);

            cart.Clear();
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Cleared cart. UserId: {UserId}, ItemsRemoved: {Count}",
                request.UserId, itemCount);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error clearing cart. UserId: {UserId}",
                request.UserId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

