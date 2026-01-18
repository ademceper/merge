using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Cart.Commands.AddItemToCart;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.MoveToCart;

public class MoveToCartCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<MoveToCartCommandHandler> logger) : IRequestHandler<MoveToCartCommand, bool>
{

    public async Task<bool> Handle(MoveToCartCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var item = await context.Set<SavedCartItem>()
                .Include(sci => sci.Product)
                .FirstOrDefaultAsync(sci => sci.Id == request.ItemId &&
                                          sci.UserId == request.UserId, cancellationToken);

            if (item is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            var addItemCommand = new AddItemToCartCommand(request.UserId, item.ProductId, item.Quantity);
            await mediator.Send(addItemCommand, cancellationToken);
            
            item.MarkAsDeleted();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "SavedCartItem sepete tasima hatasi. UserId: {UserId}, ItemId: {ItemId}",
                request.UserId, request.ItemId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

