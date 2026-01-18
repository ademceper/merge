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

namespace Merge.Application.Catalog.Commands.ReleaseStock;

public class ReleaseStockCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IRepository inventoryRepository,
    ICacheService cache,
    ILogger<ReleaseStockCommandHandler> logger) : IRequestHandler<ReleaseStockCommand, bool>
{
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public async Task<bool> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Releasing stock. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, OrderId: {OrderId}, UserId: {UserId}",
            request.ProductId, request.WarehouseId, request.Quantity, request.OrderId, request.PerformedBy);

        if (request.Quantity <= 0)
        {
            throw new ValidationException("Serbest bırakılacak miktar 0'dan büyük olmalıdır.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                logger.LogWarning("IDOR attempt: User {UserId} tried to release stock for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için stok serbest bırakma yetkiniz yok.");
            }

            var inventory = await context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.WarehouseId, cancellationToken);

            if (inventory == null)
            {
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            var quantityBefore = inventory.Quantity;
            inventory.ReleaseAndReduce(request.Quantity);

            await inventoryRepository.UpdateAsync(inventory, cancellationToken);

            var stockMovement = StockMovement.Create(
                inventory.Id,
                request.ProductId,
                request.WarehouseId,
                StockMovementType.Sale,
                request.Quantity, // Absolute value, negative is handled by MovementType
                quantityBefore,
                inventory.Quantity,
                request.PerformedBy,
                null, // referenceNumber
                request.OrderId, // referenceId
                $"Stock released for order fulfillment {request.OrderId}",
                null, // fromWarehouseId
                null); // toWarehouseId

            await context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Successfully released stock. ProductId: {ProductId}, Quantity: {Quantity}, OrderId: {OrderId}",
                request.ProductId, request.Quantity, request.OrderId);

            await cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}", cancellationToken);
            await cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while releasing stock. ProductId: {ProductId}", request.ProductId);
            throw new BusinessException("Stok serbest bırakma çakışması. Başka bir kullanıcı aynı envanteri güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error releasing stock. ProductId: {ProductId}", request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

