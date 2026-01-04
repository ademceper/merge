using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Order;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.User;


namespace Merge.Application.Services.Order;

public class OrderSplitService : IOrderSplitService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderSplitService> _logger;

    public OrderSplitService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IOrderService orderService,
        IMapper mapper,
        ILogger<OrderSplitService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _orderService = orderService;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrderSplitDto> SplitOrderAsync(Guid orderId, CreateOrderSplitDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Sipariş bölme işlemi başlatılıyor. OrderId: {OrderId}, ItemsCount: {ItemsCount}",
            orderId, dto.Items?.Count ?? 0);

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var originalOrder = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (originalOrder == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        if (originalOrder.Status != OrderStatus.Pending && originalOrder.Status != OrderStatus.Processing)
        {
            throw new BusinessException("Sipariş sadece Beklemede veya İşleniyor durumundayken bölünebilir.");
        }

        // Validate split items
        var totalSplitQuantity = 0;
        foreach (var item in dto.Items)
        {
            var orderItem = originalOrder.OrderItems.FirstOrDefault(oi => oi.Id == item.OrderItemId);
            if (orderItem == null)
            {
                throw new NotFoundException("Sipariş kalemi", item.OrderItemId);
            }
            if (item.Quantity > orderItem.Quantity)
            {
                throw new ValidationException($"Sipariş kalemi {item.OrderItemId} için mevcut miktardan fazla bölünemez.");
            }
            totalSplitQuantity += item.Quantity;
        }

        if (totalSplitQuantity == 0)
        {
            throw new ValidationException("En az bir kalem bölünmelidir.");
        }

        // ✅ ARCHITECTURE: Transaction kullan - kritik multi-entity işlem
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            // Address entity'sini çek
            var addressId = dto.NewAddressId ?? originalOrder.AddressId;
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId, cancellationToken);
            
            if (address == null)
            {
                throw new NotFoundException("Adres", addressId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var splitOrder = OrderEntity.Create(originalOrder.UserId, addressId, address);
            
            // Split order için özel ayarlar
            // Not: Order.Create factory method OrderNumber'ı otomatik oluşturuyor
            // Split order için özel order number gerekiyorsa, Order entity'sine SetOrderNumber method'u eklenebilir
            // Şimdilik factory method'un oluşturduğu order number kullanılıyor

            // Calculate split order totals
            decimal splitSubTotal = 0;

            foreach (var item in dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                
                // Product'ı çek (AddItem için gerekli)
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == originalItem.ProductId, cancellationToken);
                
                if (product == null)
                {
                    throw new NotFoundException("Ürün", originalItem.ProductId);
                }

                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                splitOrder.AddItem(product, item.Quantity);
                splitSubTotal += originalItem.UnitPrice * item.Quantity;

                // Update original order item quantity
                originalItem.Quantity -= item.Quantity;
                originalItem.TotalPrice = originalItem.UnitPrice * originalItem.Quantity;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var shippingCost = new Money(originalOrder.ShippingCost);
            splitOrder.SetShippingCost(shippingCost);
            
            var tax = new Money(splitSubTotal * (originalOrder.Tax / originalOrder.SubTotal));
            splitOrder.SetTax(tax);

            // ✅ PERFORMANCE: Memory'de Sum kullanılıyor - Ancak bu business logic için gerekli (order items zaten Include ile yüklenmiş)
            // Update original order totals - Domain method'lar kullanılamıyor çünkü Order entity'sinde RecalculateTotals private
            // Bu durumda service layer'da manuel hesaplama yapılıyor (Order entity'sine public RecalculateTotals eklenebilir)
            // Şimdilik manuel hesaplama yapılıyor

            // ✅ BOLUM 1.1: Rich Domain Model - OrderItem'lar AddItem ile Order.OrderItems collection'ına ekleniyor
            var splitOrderItems = new List<OrderItem>(); // ✅ FIX: Declare for later use
            foreach (var item in dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                var product = await _context.Set<ProductEntity>().FirstOrDefaultAsync(p => p.Id == originalItem.ProductId, cancellationToken);
                if (product != null)
                {
                    splitOrder.AddItem(product, item.Quantity);
                    splitOrderItems.Add(splitOrder.OrderItems.Last());
                }
            }
            
            await _context.Orders.AddAsync(splitOrder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create OrderSplit record
            var orderSplit = new OrderSplit
            {
                OriginalOrderId = originalOrder.Id,
                SplitOrderId = splitOrder.Id,
                SplitReason = dto.SplitReason,
                NewAddressId = dto.NewAddressId,
                Status = OrderSplitStatus.Pending
            };

            await _context.Set<OrderSplit>().AddAsync(orderSplit, cancellationToken);

            // Create OrderSplitItem records
            var splitItemRecords = new List<OrderSplitItem>();
            foreach (var item in dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                var splitItem = splitOrderItems.First(si => si.ProductId == originalItem.ProductId && si.Quantity == item.Quantity);

                splitItemRecords.Add(new OrderSplitItem
                {
                    OrderSplitId = orderSplit.Id,
                    OriginalOrderItemId = originalItem.Id,
                    SplitOrderItemId = splitItem.Id,
                    Quantity = item.Quantity
                });
            }

            await _context.Set<OrderSplitItem>().AddRangeAsync(splitItemRecords, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ ARCHITECTURE: Transaction commit
            await _unitOfWork.CommitTransactionAsync();

            // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
            _logger.LogInformation(
                "Sipariş başarıyla bölündü. OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                orderId, splitOrder.Id);

            // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
            orderSplit = await _context.Set<OrderSplit>()
                .AsNoTracking()
                .Include(s => s.OriginalOrder)
                .Include(s => s.SplitOrder)
                .Include(s => s.NewAddress)
                .Include(s => s.OrderSplitItems)
                    .ThenInclude(si => si.OriginalOrderItem)
                        .ThenInclude(oi => oi.Product)
                .Include(s => s.OrderSplitItems)
                    .ThenInclude(si => si.SplitOrderItem)
                .FirstOrDefaultAsync(s => s.Id == orderSplit.Id, cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<OrderSplitDto>(orderSplit);
        }
        catch (Exception ex)
        {
            // ✅ ARCHITECTURE: Transaction rollback on error
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Sipariş bölme işlemi başarısız. OrderId: {OrderId}", orderId);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrderSplitDto?> GetSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var split = await _context.Set<OrderSplit>()
            .AsNoTracking()
            .Include(s => s.OriginalOrder)
            .Include(s => s.SplitOrder)
            .Include(s => s.NewAddress)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.OriginalOrderItem)
                    .ThenInclude(oi => oi.Product)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.SplitOrderItem)
            .FirstOrDefaultAsync(s => s.Id == splitId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return split != null ? _mapper.Map<OrderSplitDto>(split) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderSplitDto>> GetOrderSplitsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var splits = await _context.Set<OrderSplit>()
            .AsNoTracking()
            .Include(s => s.OriginalOrder)
            .Include(s => s.SplitOrder)
            .Include(s => s.NewAddress)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.OriginalOrderItem)
                    .ThenInclude(oi => oi.Product)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.SplitOrderItem)
            .Where(s => s.OriginalOrderId == orderId)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası foreach içinde MapToDto YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderSplitDto>> GetSplitOrdersAsync(Guid splitOrderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var splits = await _context.Set<OrderSplit>()
            .AsNoTracking()
            .Include(s => s.OriginalOrder)
            .Include(s => s.SplitOrder)
            .Include(s => s.NewAddress)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.OriginalOrderItem)
                    .ThenInclude(oi => oi.Product)
            .Include(s => s.OrderSplitItems)
                .ThenInclude(si => si.SplitOrderItem)
            .Where(s => s.SplitOrderId == splitOrderId)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası foreach içinde MapToDto YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CancelSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var split = await _context.Set<OrderSplit>()
            .Include(s => s.SplitOrder)
            .Include(s => s.OriginalOrder)
            .FirstOrDefaultAsync(s => s.Id == splitId, cancellationToken);

        if (split == null) return false;

        if (split.SplitOrder.Status != OrderStatus.Pending)
        {
            throw new BusinessException("Beklemede durumunda olmayan bölünmüş sipariş iptal edilemez.");
        }

        // ✅ PERFORMANCE: Removed manual !si.IsDeleted (Global Query Filter)
        // Merge items back to original order
        var splitItems = await _context.Set<OrderSplitItem>()
            .Include(si => si.OriginalOrderItem)
                .ThenInclude(oi => oi.Product)
            .Include(si => si.SplitOrderItem)
                .ThenInclude(oi => oi.Product)
            .Where(si => si.OrderSplitId == splitId)
            .ToListAsync(cancellationToken);

        foreach (var splitItem in splitItems)
        {
            var originalItem = splitItem.OriginalOrderItem;
            var splitOrderItem = splitItem.SplitOrderItem;
            
            originalItem.Quantity += splitItem.Quantity;
            originalItem.TotalPrice = originalItem.UnitPrice * originalItem.Quantity;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        // Recalculate original order totals
        var originalOrder = split.OriginalOrder;
        originalOrder.RecalculateTotals();

        // Delete split order
        split.SplitOrder.IsDeleted = true;
        split.Status = OrderSplitStatus.Cancelled;
        split.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CompleteSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var split = await _context.Set<OrderSplit>()
            .FirstOrDefaultAsync(s => s.Id == splitId, cancellationToken);

        if (split == null) return false;

        split.Status = OrderSplitStatus.Completed;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

}

