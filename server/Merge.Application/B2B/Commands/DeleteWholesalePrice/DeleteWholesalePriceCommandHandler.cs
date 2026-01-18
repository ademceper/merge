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

public class DeleteWholesalePriceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteWholesalePriceCommandHandler> logger) : IRequestHandler<DeleteWholesalePriceCommand, bool>
{

    public async Task<bool> Handle(DeleteWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting wholesale price. WholesalePriceId: {WholesalePriceId}", request.Id);

        var price = await context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        if (price is null)
        {
            logger.LogWarning("Wholesale price not found with Id: {WholesalePriceId}", request.Id);
            return false;
        }

        price.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Wholesale price deleted successfully. WholesalePriceId: {WholesalePriceId}", request.Id);
        return true;
    }
}

