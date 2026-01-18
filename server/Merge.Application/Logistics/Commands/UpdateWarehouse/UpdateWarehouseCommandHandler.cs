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

namespace Merge.Application.Logistics.Commands.UpdateWarehouse;

public class UpdateWarehouseCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateWarehouseCommandHandler> logger) : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating warehouse. WarehouseId: {WarehouseId}", request.Id);

        var warehouse = await context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse is null)
        {
            logger.LogWarning("Warehouse not found. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedWarehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (updatedWarehouse is null)
        {
            logger.LogWarning("Warehouse not found after update. WarehouseId: {WarehouseId}", request.Id);
            throw new NotFoundException("Depo", request.Id);
        }

        logger.LogInformation("Warehouse updated successfully. WarehouseId: {WarehouseId}", request.Id);

        return mapper.Map<WarehouseDto>(updatedWarehouse);
    }
}

