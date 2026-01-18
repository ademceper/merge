using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Inventory.Inventory>;

namespace Merge.Application.Catalog.Commands.ReserveStock;

public class ReserveStockCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IRepository inventoryRepository,
    ICacheService cache,
    ILogger<ReserveStockCommandHandler> logger) : IRequestHandler<ReserveStockCommand, bool>
{
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<bool> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reserving stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, OrderId: {OrderId}, UserId: {UserId}",
            request.ProductId, request.WarehouseId, request.Quantity, request.OrderId, request.PerformedBy);

        if (request.Quantity <= 0)
        {
            throw new ValidationException("Rezerve edilecek miktar 0'dan büyük olmalıdır.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to reserve stock for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için stok rezervasyon yetkiniz yok.");
            }

            var inventory = await context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.WarehouseId, cancellationToken);

            if (inventory is null)
            {
                logger.LogWarning("Inventory not found for reservation. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    request.ProductId, request.WarehouseId);
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            var quantityBefore = inventory.Quantity;
            inventory.Reserve(request.Quantity);
            await inventoryRepository.UpdateAsync(inventory, cancellationToken);

            var stockMovement = StockMovement.Create(
                inventory.Id,
                request.ProductId,
                request.WarehouseId,
                StockMovementType.Reserved,
                request.Quantity,
                quantityBefore,
                inventory.Quantity, // Total quantity doesn't change, only reserved
                request.PerformedBy,
                null, // referenceNumber
                request.OrderId, // referenceId
                $"Stock reserved for order {request.OrderId}",
                null, // fromWarehouseId
                null); // toWarehouseId

            await context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Successfully reserved stock. ProductId: {ProductId}, Quantity: {Quantity}, OrderId: {OrderId}",
                request.ProductId, request.Quantity, request.OrderId);

            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}", cancellationToken);
            await cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while reserving stock. ProductId: {ProductId}", request.ProductId);
            throw new BusinessException("Stok rezervasyon çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reserving stock. ProductId: {ProductId}", request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

