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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ClearSavedItemsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ClearSavedItemsCommandHandler> logger) : IRequestHandler<ClearSavedItemsCommand, bool>
{

    public async Task<bool> Handle(ClearSavedItemsCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Use bulk update instead of foreach DeleteAsync (N+1 fix)
        // ✅ PERFORMANCE: Removed manual !sci.IsDeleted check (Global Query Filter handles it)
        var items = await context.Set<SavedCartItem>()
            .Where(sci => sci.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
                item.MarkAsDeleted();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
        }

        return true;
    }
}

