using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.DeleteWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteWholesalePriceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteWholesalePriceCommandHandler> logger) : IRequestHandler<DeleteWholesalePriceCommand, bool>
{

    public async Task<bool> Handle(DeleteWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting wholesale price. WholesalePriceId: {WholesalePriceId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var price = await context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        if (price == null)
        {
            logger.LogWarning("Wholesale price not found with Id: {WholesalePriceId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Delete method (soft delete + domain event)
        price.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Wholesale price deleted successfully. WholesalePriceId: {WholesalePriceId}", request.Id);
        return true;
    }
}

