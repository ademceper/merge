using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Commands.UpdateDeliveryTimeEstimation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateDeliveryTimeEstimationCommandHandler : IRequestHandler<UpdateDeliveryTimeEstimationCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateDeliveryTimeEstimationCommandHandler> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public UpdateDeliveryTimeEstimationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateDeliveryTimeEstimationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateDeliveryTimeEstimationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating delivery time estimation. EstimationId: {EstimationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var estimation = await _context.Set<DeliveryTimeEstimation>()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (estimation == null)
        {
            _logger.LogWarning("Delivery time estimation not found. EstimationId: {EstimationId}", request.Id);
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

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Delivery time estimation updated successfully. EstimationId: {EstimationId}", request.Id);
        return Unit.Value;
    }
}

