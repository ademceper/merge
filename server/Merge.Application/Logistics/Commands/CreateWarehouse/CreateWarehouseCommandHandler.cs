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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CreateWarehouseCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateWarehouseCommandHandler> logger) : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating warehouse. Code: {Code}, Name: {Name}", request.Code, request.Name);

        // ✅ PERFORMANCE: AsNoTracking - Check if code already exists
        var existingWarehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .AnyAsync(w => w.Code == request.Code, cancellationToken);

        if (existingWarehouse)
        {
            logger.LogWarning("Warehouse with code already exists. Code: {Code}", request.Code);
            throw new BusinessException($"Bu kod ile depo zaten mevcut: '{request.Code}'");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var createdWarehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == warehouse.Id, cancellationToken);

        if (createdWarehouse == null)
        {
            logger.LogWarning("Warehouse not found after creation. WarehouseId: {WarehouseId}", warehouse.Id);
            throw new NotFoundException("Depo", warehouse.Id);
        }

        logger.LogInformation("Warehouse created successfully. WarehouseId: {WarehouseId}, Code: {Code}", warehouse.Id, request.Code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<WarehouseDto>(createdWarehouse);
    }
}

