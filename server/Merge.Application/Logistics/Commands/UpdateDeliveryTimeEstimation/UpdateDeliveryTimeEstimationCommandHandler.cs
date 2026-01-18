using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.UpdateDeliveryTimeEstimation;

public class UpdateDeliveryTimeEstimationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateDeliveryTimeEstimationCommandHandler> logger) : IRequestHandler<UpdateDeliveryTimeEstimationCommand, Unit>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<Unit> Handle(UpdateDeliveryTimeEstimationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating delivery time estimation. EstimationId: {EstimationId}", request.Id);

        var estimation = await context.Set<DeliveryTimeEstimation>()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (estimation == null)
        {
            logger.LogWarning("Delivery time estimation not found. EstimationId: {EstimationId}", request.Id);
            throw new NotFoundException("Teslimat süresi tahmini", request.Id);
        }

        if (request.MinDays.HasValue || request.MaxDays.HasValue || request.AverageDays.HasValue)
        {
            var minDays = request.MinDays ?? estimation.MinDays;
            var maxDays = request.MaxDays ?? estimation.MaxDays;
            var averageDays = request.AverageDays ?? estimation.AverageDays;
            estimation.UpdateDays(minDays, maxDays, averageDays);
        }

        if (request.Conditions != null)
        {
            var conditionsJson = JsonSerializer.Serialize(request.Conditions, JsonOptions);
            estimation.UpdateConditions(conditionsJson);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                estimation.Activate();
            }
            else
            {
                estimation.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Delivery time estimation updated successfully. EstimationId: {EstimationId}", request.Id);
        return Unit.Value;
    }
}

