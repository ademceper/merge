using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Logistics.Commands.CreatePickPack;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreatePickPackCommandHandler : IRequestHandler<CreatePickPackCommand, PickPackDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePickPackCommandHandler> _logger;

    public CreatePickPackCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreatePickPackCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PickPackDto> Handle(CreatePickPackCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating pick-pack. OrderId: {OrderId}, WarehouseId: {WarehouseId}", request.OrderId, request.WarehouseId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var order = await _context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // ✅ PERFORMANCE: AsNoTracking - Check if warehouse exists
        var warehouse = await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.IsActive, cancellationToken);

        if (warehouse == null)
        {
            _logger.LogWarning("Warehouse not found or inactive. WarehouseId: {WarehouseId}", request.WarehouseId);
            throw new NotFoundException("Depo", request.WarehouseId);
        }

        // ✅ PERFORMANCE: AsNoTracking - Check if pick pack already exists
        var existing = await _context.Set<PickPack>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pp => pp.OrderId == request.OrderId, cancellationToken);

        if (existing != null)
        {
            _logger.LogWarning("Pick pack already exists for order. OrderId: {OrderId}", request.OrderId);
            throw new BusinessException("Bu sipariş için zaten bir pick pack kaydı var.");
        }

        var packNumber = await GeneratePackNumberAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var pickPack = PickPack.Create(
            request.OrderId,
            request.WarehouseId,
            packNumber,
            request.Notes);

        await _context.Set<PickPack>().AddAsync(pickPack, cancellationToken);

        // Create pick pack items from order items
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var items = new List<PickPackItem>(order.OrderItems.Count);
        foreach (var orderItem in order.OrderItems)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var pickPackItem = PickPackItem.Create(
                pickPack.Id,
                orderItem.Id,
                orderItem.ProductId,
                orderItem.Quantity);
            items.Add(pickPackItem);
        }

        await _context.Set<PickPackItem>().AddRangeAsync(items, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
        var createdPickPack = await _context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(pp => pp.Id == pickPack.Id, cancellationToken);

        if (createdPickPack == null)
        {
            _logger.LogWarning("Pick pack not found after creation. PickPackId: {PickPackId}", pickPack.Id);
            throw new NotFoundException("Pick-pack", pickPack.Id);
        }

        _logger.LogInformation("Pick-pack created successfully. PickPackId: {PickPackId}, PackNumber: {PackNumber}", pickPack.Id, packNumber);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PickPackDto>(createdPickPack);
    }

    private async Task<string> GeneratePackNumberAsync(CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var existingCount = await _context.Set<PickPack>()
            .AsNoTracking()
            .CountAsync(pp => pp.PackNumber.StartsWith($"PK-{date}"), cancellationToken);

        return $"PK-{date}-{(existingCount + 1):D6}";
    }
}

