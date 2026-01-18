using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.RemoveSavedItem;

public class RemoveSavedItemCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveSavedItemCommandHandler> logger) : IRequestHandler<RemoveSavedItemCommand, bool>
{

    public async Task<bool> Handle(RemoveSavedItemCommand request, CancellationToken cancellationToken)
    {
        var item = await context.Set<SavedCartItem>()
            .FirstOrDefaultAsync(sci => sci.Id == request.ItemId &&
                                      sci.UserId == request.UserId, cancellationToken);

        if (item is null)
        {
            return false;
        }

        item.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

