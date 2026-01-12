using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.UpdatePickPackDetails;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdatePickPackDetailsCommandHandler : IRequestHandler<UpdatePickPackDetailsCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePickPackDetailsCommandHandler> _logger;

    public UpdatePickPackDetailsCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePickPackDetailsCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdatePickPackDetailsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating pick-pack details. PickPackId: {PickPackId}, Status: {Status}", request.PickPackId, request.Status);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var pickPack = await _context.Set<PickPack>()
            .FirstOrDefaultAsync(pp => pp.Id == request.PickPackId, cancellationToken);

        if (pickPack == null)
        {
            _logger.LogWarning("Pick pack not found. PickPackId: {PickPackId}", request.PickPackId);
            throw new NotFoundException("Pick-pack", request.PickPackId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.Notes != null || request.Weight.HasValue || request.Dimensions != null || request.PackageCount.HasValue)
        {
            pickPack.UpdateDetails(
                request.Notes,
                request.Weight,
                request.Dimensions,
                request.PackageCount);
        }

        // Status değişikliği için domain method'ları kullan
        if (request.Status.HasValue)
        {
            // Status transition'ları domain method'lar ile yapılmalı
            // Burada sadece direkt status set edilemez, domain method'lar kullanılmalı
            // Ancak UpdatePickPackStatusDto'da status string olarak geliyor, bu yüzden bu command'ı kaldırabiliriz
            // veya status transition'ları için ayrı command'lar kullanabiliriz
            // Şimdilik bu command'ı sadece details update için kullanacağız
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pick-pack details updated successfully. PickPackId: {PickPackId}", request.PickPackId);
        return Unit.Value;
    }
}

