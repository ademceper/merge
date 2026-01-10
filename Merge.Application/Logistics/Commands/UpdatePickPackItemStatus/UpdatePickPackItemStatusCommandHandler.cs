using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Commands.UpdatePickPackItemStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdatePickPackItemStatusCommandHandler : IRequestHandler<UpdatePickPackItemStatusCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePickPackItemStatusCommandHandler> _logger;

    public UpdatePickPackItemStatusCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePickPackItemStatusCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdatePickPackItemStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating pick-pack item status. ItemId: {ItemId}, IsPicked: {IsPicked}, IsPacked: {IsPacked}",
            request.ItemId, request.IsPicked, request.IsPacked);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var item = await _context.Set<PickPackItem>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("Pick-pack item not found. ItemId: {ItemId}", request.ItemId);
            throw new NotFoundException("Pick-pack kalemi", request.ItemId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.IsPicked.HasValue && request.IsPicked.Value && !item.IsPicked)
        {
            item.MarkAsPicked();
        }

        if (request.IsPacked.HasValue && request.IsPacked.Value && !item.IsPacked)
        {
            item.MarkAsPacked();
        }

        if (!string.IsNullOrEmpty(request.Location))
        {
            item.UpdateLocation(request.Location);
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pick-pack item status updated successfully. ItemId: {ItemId}", request.ItemId);
        return Unit.Value;
    }
}

