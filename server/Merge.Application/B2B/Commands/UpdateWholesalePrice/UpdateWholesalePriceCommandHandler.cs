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

public class UpdateWholesalePriceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateWholesalePriceCommandHandler> logger) : IRequestHandler<UpdateWholesalePriceCommand, bool>
{

    public async Task<bool> Handle(UpdateWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating wholesale price. WholesalePriceId: {WholesalePriceId}", request.Id);


        var price = await context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        if (price == null)
        {
            logger.LogWarning("Wholesale price not found with Id: {WholesalePriceId}", request.Id);
            return false;
        }

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

