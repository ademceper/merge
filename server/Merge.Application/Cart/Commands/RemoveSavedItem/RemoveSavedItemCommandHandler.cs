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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RemoveSavedItemCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveSavedItemCommandHandler> logger) : IRequestHandler<RemoveSavedItemCommand, bool>
{

    public async Task<bool> Handle(RemoveSavedItemCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !sci.IsDeleted check (Global Query Filter handles it)
        var item = await context.Set<SavedCartItem>()
            .FirstOrDefaultAsync(sci => sci.Id == request.ItemId &&
                                      sci.UserId == request.UserId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (item is null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        item.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

