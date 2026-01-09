using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Catalog.Commands.CreateInventory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, InventoryDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateInventoryCommandHandler> _logger;
    private const string CACHE_KEY_INVENTORY_BY_ID = "inventory_";
    private const string CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE = "inventory_product_warehouse_";

    public CreateInventoryCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateInventoryCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<InventoryDto> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}",
            request.ProductId, request.WarehouseId, request.Quantity);

        if (request.Quantity < 0)
        {
            throw new ValidationException("Miktar negatif olamaz.");
        }

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Inventory + StockMovement)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini oluşturabilmeli
            var product = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            // Seller kontrolü: Seller sadece kendi ürünlerinin inventory'sini oluşturabilir
            // Admin tüm ürünler için inventory oluşturabilir
            // Note: Role bilgisi command'da yok, bu yüzden sadece SellerId kontrolü yapıyoruz
            // Controller seviyesinde role kontrolü yapılıyor, burada sadece ownership kontrolü
            if (product.SellerId.HasValue && product.SellerId != request.PerformedBy)
            {
                _logger.LogWarning("IDOR attempt: User {UserId} tried to create inventory for product {ProductId} owned by {OwnerId}",
                    request.PerformedBy, request.ProductId, product.SellerId);
                throw new BusinessException("Bu ürün için envanter oluşturma yetkiniz yok.");
            }

            // Check if inventory already exists for this product-warehouse combination
            var existingInventory = await _context.Set<Inventory>()
                .AsNoTracking()
                .AnyAsync(i => i.ProductId == request.ProductId &&
                              i.WarehouseId == request.WarehouseId, cancellationToken);

            if (existingInventory)
            {
                _logger.LogWarning("Attempted to create duplicate inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                    request.ProductId, request.WarehouseId);
                throw new BusinessException("Bu ürün-depo kombinasyonu için envanter zaten mevcut.");
            }

            // ✅ BOLUM 1.1: Factory Method kullanımı
            var inventory = Inventory.Create(
                request.ProductId,
                request.WarehouseId,
                request.Quantity,
                request.MinimumStockLevel,
                request.MaximumStockLevel,
                request.UnitCost,
                request.Location);

            await _context.Set<Inventory>().AddAsync(inventory, cancellationToken);

            // Create initial stock movement
            if (request.Quantity > 0)
            {
                // ⚠️ NOT: StockMovement entity anemic (factory method yok), object initializer kullanılıyor
                var stockMovement = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    InventoryId = inventory.Id,
                    ProductId = inventory.ProductId,
                    WarehouseId = inventory.WarehouseId,
                    MovementType = StockMovementType.Receipt,
                    Quantity = request.Quantity,
                    QuantityBefore = 0,
                    QuantityAfter = request.Quantity,
                    Notes = "Initial inventory creation",
                    PerformedBy = request.PerformedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
            var reloadedInventory = await _context.Set<Inventory>()
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == inventory.Id, cancellationToken);

            if (reloadedInventory == null)
            {
                _logger.LogWarning("Inventory {InventoryId} not found after creation", inventory.Id);
                return _mapper.Map<InventoryDto>(inventory);
            }

            _logger.LogInformation("Successfully created inventory with Id: {InventoryId} for ProductId: {ProductId}",
                reloadedInventory.Id, request.ProductId);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_ID}{reloadedInventory.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_INVENTORY_BY_PRODUCT_WAREHOUSE}{request.ProductId}_{request.WarehouseId}", cancellationToken);
            await _cache.RemoveAsync($"inventories_by_product_{request.ProductId}", cancellationToken); // Invalidate product inventories list cache

            return _mapper.Map<InventoryDto>(reloadedInventory);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating inventory for ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

