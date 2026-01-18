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

namespace Merge.Application.Logistics.Commands.CreateWarehouse;

public class CreateWarehouseCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateWarehouseCommandHandler> logger) : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating warehouse. Code: {Code}, Name: {Name}", request.Code, request.Name);

        var existingWarehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .AnyAsync(w => w.Code == request.Code, cancellationToken);

        if (existingWarehouse)
        {
            logger.LogWarning("Warehouse with code already exists. Code: {Code}", request.Code);
            throw new BusinessException($"Bu kod ile depo zaten mevcut: '{request.Code}'");
        }

        var warehouse = Warehouse.Create(
            request.Name,
            request.Code,
            request.Address,
            request.City,
            request.Country,
            request.PostalCode,
            request.ContactPerson,
            request.ContactPhone,
            request.ContactEmail,
            request.Capacity,
            request.Description);

        await context.Set<Warehouse>().AddAsync(warehouse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdWarehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == warehouse.Id, cancellationToken);

        if (createdWarehouse is null)
        {
            logger.LogWarning("Warehouse not found after creation. WarehouseId: {WarehouseId}", warehouse.Id);
            throw new NotFoundException("Depo", warehouse.Id);
        }

        logger.LogInformation("Warehouse created successfully. WarehouseId: {WarehouseId}, Code: {Code}", warehouse.Id, request.Code);

        return mapper.Map<WarehouseDto>(createdWarehouse);
    }
}

