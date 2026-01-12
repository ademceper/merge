using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.StartPacking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class StartPackingCommandHandler : IRequestHandler<StartPackingCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartPackingCommandHandler> _logger;

    public StartPackingCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<StartPackingCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(StartPackingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting packing. PickPackId: {PickPackId}, UserId: {UserId}", request.PickPackId, request.UserId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            _logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        pickPack.StartPacking(request.UserId);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Packing started successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

