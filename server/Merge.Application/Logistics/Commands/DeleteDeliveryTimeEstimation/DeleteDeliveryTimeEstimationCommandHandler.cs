using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.DeleteDeliveryTimeEstimation;

public class DeleteDeliveryTimeEstimationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteDeliveryTimeEstimationCommandHandler> logger) : IRequestHandler<DeleteDeliveryTimeEstimationCommand, Unit>
{

    public async Task<Unit> Handle(DeleteDeliveryTimeEstimationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting delivery time estimation. EstimationId: {EstimationId}", request.Id);

        var estimation = await context.Set<DeliveryTimeEstimation>()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (estimation is null)
        {
            logger.LogWarning("Delivery time estimation not found for deletion. EstimationId: {EstimationId}", request.Id);
            throw new NotFoundException("Teslimat süresi tahmini", request.Id);
        }

        estimation.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Delivery time estimation deleted successfully. EstimationId: {EstimationId}", request.Id);
        return Unit.Value;
    }
}

