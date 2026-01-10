using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Commands.UpdateWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateWarehouseCommandHandler> _logger;

    public UpdateWarehouseCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateWarehouseCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating warehouse. WarehouseId: {WarehouseId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var warehouse = await _context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
        {
            _logger.LogWarning("Warehouse not found. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        warehouse.UpdateDetails(
            request.Name,
            request.Address,
            request.City,
            request.Country,
            request.PostalCode,
            request.ContactPerson,
            request.ContactPhone,
            request.ContactEmail,
            request.Capacity,
            request.Description);

        // IsActive durumunu güncelle
        if (request.IsActive && !warehouse.IsActive)
        {
            warehouse.Activate();
        }
        else if (!request.IsActive && warehouse.IsActive)
        {
            warehouse.Deactivate();
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var updatedWarehouse = await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (updatedWarehouse == null)
        {
            _logger.LogWarning("Warehouse not found after update. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        _logger.LogInformation("Warehouse updated successfully. WarehouseId: {WarehouseId}", request.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<WarehouseDto>(updatedWarehouse);
    }
}

