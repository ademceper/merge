using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.UpdateWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateWholesalePriceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateWholesalePriceCommandHandler> logger) : IRequestHandler<UpdateWholesalePriceCommand, bool>
{

    public async Task<bool> Handle(UpdateWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating wholesale price. WholesalePriceId: {WholesalePriceId}", request.Id);

        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, handler'da tekrar validation gereksiz

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var price = await context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        if (price == null)
        {
            logger.LogWarning("Wholesale price not found with Id: {WholesalePriceId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        price.UpdateQuantityRange(request.Dto.MinQuantity, request.Dto.MaxQuantity);
        price.UpdatePrice(request.Dto.Price);
        price.UpdateDates(request.Dto.StartDate, request.Dto.EndDate);
        if (request.Dto.IsActive)
            price.Activate();
        else
            price.Deactivate();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Wholesale price updated successfully. WholesalePriceId: {WholesalePriceId}", request.Id);
        return true;
    }
}

