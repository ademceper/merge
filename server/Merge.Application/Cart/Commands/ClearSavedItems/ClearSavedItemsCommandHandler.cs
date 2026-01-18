using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.ClearSavedItems;

public class ClearSavedItemsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ClearSavedItemsCommandHandler> logger) : IRequestHandler<ClearSavedItemsCommand, bool>
{

    public async Task<bool> Handle(ClearSavedItemsCommand request, CancellationToken cancellationToken)
    {
        var items = await context.Set<SavedCartItem>()
            .Where(sci => sci.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                item.MarkAsDeleted();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
        }

        return true;
    }
}

