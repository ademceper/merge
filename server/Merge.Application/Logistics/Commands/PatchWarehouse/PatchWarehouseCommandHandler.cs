using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.PatchWarehouse;

/// <summary>
/// Handler for PatchWarehouseCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchWarehouseCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchWarehouseCommandHandler> logger) : IRequestHandler<PatchWarehouseCommand, WarehouseDto>
{
    public async Task<WarehouseDto> Handle(PatchWarehouseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching warehouse. WarehouseId: {WarehouseId}", request.Id);

        var warehouse = await context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
        {
            logger.LogWarning("Warehouse not found. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        // Apply partial updates - get existing values if not provided
        var name = request.PatchDto.Name ?? warehouse.Name;
        var address = request.PatchDto.Address ?? warehouse.Address;
        var city = request.PatchDto.City ?? warehouse.City;
        var country = request.PatchDto.Country ?? warehouse.Country;
        var postalCode = request.PatchDto.PostalCode ?? warehouse.PostalCode;
        var contactPerson = request.PatchDto.ContactPerson ?? warehouse.ContactPerson;
        var contactPhone = request.PatchDto.ContactPhone ?? warehouse.ContactPhone;
        var contactEmail = request.PatchDto.ContactEmail ?? warehouse.ContactEmail;
        var capacity = request.PatchDto.Capacity ?? warehouse.Capacity;
        var description = request.PatchDto.Description ?? warehouse.Description;

        warehouse.UpdateDetails(
            name,
            address,
            city,
            country,
            postalCode,
            contactPerson,
            contactPhone,
            contactEmail,
            capacity,
            description);

        // IsActive durumunu güncelle
        if (request.PatchDto.IsActive.HasValue)
        {
            if (request.PatchDto.IsActive.Value && !warehouse.IsActive)
            {
                warehouse.Activate();
            }
            else if (!request.PatchDto.IsActive.Value && warehouse.IsActive)
            {
                warehouse.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Warehouse patched successfully. WarehouseId: {WarehouseId}", request.Id);

        return mapper.Map<WarehouseDto>(warehouse);
    }
}
