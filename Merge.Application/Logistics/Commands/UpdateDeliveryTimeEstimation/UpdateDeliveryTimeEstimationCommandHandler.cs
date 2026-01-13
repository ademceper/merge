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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class UpdateDeliveryTimeEstimationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateDeliveryTimeEstimationCommandHandler> logger) : IRequestHandler<UpdateDeliveryTimeEstimationCommand, Unit>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<Unit> Handle(UpdateDeliveryTimeEstimationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating delivery time estimation. EstimationId: {EstimationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var estimation = await context.Set<DeliveryTimeEstimation>()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (estimation == null)
        {
            logger.LogWarning("Delivery time estimation not found. EstimationId: {EstimationId}", request.Id);
            throw new NotFoundException("Teslimat süresi tahmini", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
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

        // ✅ BOLUM 7.1.6: Pattern Matching - Switch expression kullanımı
        if (request.IsActive.HasValue)
        {
            _ = request.IsActive.Value switch
            {
                true => estimation.Activate(),
                false => estimation.Deactivate()
            };
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Delivery time estimation updated successfully. EstimationId: {EstimationId}", request.Id);
        return Unit.Value;
    }
}

