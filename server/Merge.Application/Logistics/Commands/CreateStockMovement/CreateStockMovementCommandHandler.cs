using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.CreateStockMovement;

public class CreateStockMovementCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateStockMovementCommandHandler> logger) : IRequestHandler<CreateStockMovementCommand, StockMovementDto>
{

    public async Task<StockMovementDto> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating stock movement. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, MovementType: {MovementType}",
            request.ProductId, request.WarehouseId, request.Quantity, request.MovementType);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get inventory
            var inventory = await context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.WarehouseId, cancellationToken);

            if (inventory is null)
            {
                logger.LogWarning("Inventory not found. ProductId: {ProductId}, WarehouseId: {WarehouseId}", request.ProductId, request.WarehouseId);
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            var quantityBefore = inventory.Quantity;
            var quantityAfter = quantityBefore + request.Quantity;

            if (quantityAfter < 0)
            {
                logger.LogWarning("Stock quantity would be negative. QuantityBefore: {QuantityBefore}, Quantity: {Quantity}", quantityBefore, request.Quantity);
                throw new ValidationException("Stok miktarı negatif olamaz.");
            }

            var quantityChange = quantityAfter - inventory.Quantity;
            inventory.AdjustQuantity(quantityChange);

            var stockMovement = StockMovement.Create(
                inventory.Id,
                request.ProductId,
                request.WarehouseId,
                request.MovementType,
                request.Quantity,
                quantityBefore,
                quantityAfter,
                request.PerformedBy,
                request.ReferenceNumber,
                request.ReferenceId,
                request.Notes,
                request.FromWarehouseId,
                request.ToWarehouseId);

            await context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Stock movement created successfully. StockMovementId: {StockMovementId}", stockMovement.Id);

            var createdMovement = await context.Set<StockMovement>()
                .AsNoTracking()
                .Include(sm => sm.Product)
                .Include(sm => sm.Warehouse)
                .Include(sm => sm.User)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .FirstOrDefaultAsync(sm => sm.Id == stockMovement.Id, cancellationToken);

            if (createdMovement is null)
            {
                logger.LogWarning("Stock movement not found after creation. StockMovementId: {StockMovementId}", stockMovement.Id);
                throw new NotFoundException("Stok hareketi", stockMovement.Id);
            }

            return mapper.Map<StockMovementDto>(createdMovement);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Error creating stock movement. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }
}

