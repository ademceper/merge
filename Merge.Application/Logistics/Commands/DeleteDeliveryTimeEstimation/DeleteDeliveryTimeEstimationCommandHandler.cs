using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Commands.DeleteDeliveryTimeEstimation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteDeliveryTimeEstimationCommandHandler : IRequestHandler<DeleteDeliveryTimeEstimationCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDeliveryTimeEstimationCommandHandler> _logger;

    public DeleteDeliveryTimeEstimationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteDeliveryTimeEstimationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteDeliveryTimeEstimationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting delivery time estimation. EstimationId: {EstimationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var estimation = await _context.Set<DeliveryTimeEstimation>()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (estimation == null)
        {
            _logger.LogWarning("Delivery time estimation not found for deletion. EstimationId: {EstimationId}", request.Id);
            throw new NotFoundException("Teslimat süresi tahmini", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        estimation.MarkAsDeleted();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Delivery time estimation deleted successfully. EstimationId: {EstimationId}", request.Id);
        return Unit.Value;
    }
}

