using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.CreatePickPack;

public class CreatePickPackCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreatePickPackCommandHandler> logger) : IRequestHandler<CreatePickPackCommand, PickPackDto>
{

    public async Task<PickPackDto> Handle(CreatePickPackCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating pick-pack. OrderId: {OrderId}, WarehouseId: {WarehouseId}", request.OrderId, request.WarehouseId);

        var order = await context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        var warehouse = await context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.IsActive, cancellationToken);

        if (warehouse is null)
        {
            logger.LogWarning("Warehouse not found or inactive. WarehouseId: {WarehouseId}", request.WarehouseId);
            throw new NotFoundException("Depo", request.WarehouseId);
        }

        var existing = await context.Set<PickPack>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pp => pp.OrderId == request.OrderId, cancellationToken);

        if (existing is not null)
        {
            logger.LogWarning("Pick pack already exists for order. OrderId: {OrderId}", request.OrderId);
            throw new BusinessException("Bu sipariş için zaten bir pick pack kaydı var.");
        }

        var packNumber = await GeneratePackNumberAsync(cancellationToken);

        var pickPack = PickPack.Create(
            request.OrderId,
            request.WarehouseId,
            packNumber,
            request.Notes);

        await context.Set<PickPack>().AddAsync(pickPack, cancellationToken);

        // Create pick pack items from order items
        var items = new List<PickPackItem>(order.OrderItems.Count);
        foreach (var orderItem in order.OrderItems)
        {
            var pickPackItem = PickPackItem.Create(
                pickPack.Id,
                orderItem.Id,
                orderItem.ProductId,
                orderItem.Quantity);
            items.Add(pickPackItem);
        }

        await context.Set<PickPackItem>().AddRangeAsync(items, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdPickPack = await context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(pp => pp.Id == pickPack.Id, cancellationToken);

        if (createdPickPack is null)
        {
            logger.LogWarning("Pick pack not found after creation. PickPackId: {PickPackId}", pickPack.Id);
            throw new NotFoundException("Pick-pack", pickPack.Id);
        }

        logger.LogInformation("Pick-pack created successfully. PickPackId: {PickPackId}, PackNumber: {PackNumber}", pickPack.Id, packNumber);

        return mapper.Map<PickPackDto>(createdPickPack);
    }

    private async Task<string> GeneratePackNumberAsync(CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var existingCount = await context.Set<PickPack>()
            .AsNoTracking()
            .CountAsync(pp => pp.PackNumber.StartsWith($"PK-{date}"), cancellationToken);

        return $"PK-{date}-{(existingCount + 1):D6}";
    }
}

