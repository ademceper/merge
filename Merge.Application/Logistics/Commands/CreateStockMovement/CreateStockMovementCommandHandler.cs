using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Commands.CreateStockMovement;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateStockMovementCommandHandler : IRequestHandler<CreateStockMovementCommand, StockMovementDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStockMovementCommandHandler> _logger;

    public CreateStockMovementCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateStockMovementCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<StockMovementDto> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating stock movement. ProductId: {ProductId}, WarehouseId: {WarehouseId}, Quantity: {Quantity}, MovementType: {MovementType}",
            request.ProductId, request.WarehouseId, request.Quantity, request.MovementType);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Inventory update + StockMovement create)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // Get inventory
            var inventory = await _context.Set<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId &&
                                        i.WarehouseId == request.WarehouseId, cancellationToken);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found. ProductId: {ProductId}, WarehouseId: {WarehouseId}", request.ProductId, request.WarehouseId);
                throw new NotFoundException("Envanter", Guid.Empty);
            }

            var quantityBefore = inventory.Quantity;
            var quantityAfter = quantityBefore + request.Quantity;

            if (quantityAfter < 0)
            {
                _logger.LogWarning("Stock quantity would be negative. QuantityBefore: {QuantityBefore}, Quantity: {Quantity}", quantityBefore, request.Quantity);
                throw new ValidationException("Stok miktarı negatif olamaz.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            var quantityChange = quantityAfter - inventory.Quantity;
            inventory.AdjustQuantity(quantityChange);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

            await _context.Set<StockMovement>().AddAsync(stockMovement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Stock movement created successfully. StockMovementId: {StockMovementId}", stockMovement.Id);

            // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
            var createdMovement = await _context.Set<StockMovement>()
                .AsNoTracking()
                .Include(sm => sm.Product)
                .Include(sm => sm.Warehouse)
                .Include(sm => sm.User)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .FirstOrDefaultAsync(sm => sm.Id == stockMovement.Id, cancellationToken);

            if (createdMovement == null)
            {
                _logger.LogWarning("Stock movement not found after creation. StockMovementId: {StockMovementId}", stockMovement.Id);
                throw new NotFoundException("Stok hareketi", stockMovement.Id);
            }

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<StockMovementDto>(createdMovement);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating stock movement. ProductId: {ProductId}, WarehouseId: {WarehouseId}",
                request.ProductId, request.WarehouseId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }
}

