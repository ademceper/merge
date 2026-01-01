using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Order;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
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

    public async Task<OrderSplitDto> SplitOrderAsync(Guid orderId, CreateOrderSplitDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        _logger.LogInformation("Sipariş bölme işlemi başlatılıyor. OrderId: {OrderId}", orderId);

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var originalOrder = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId);

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
            // Create new split order
            var splitOrderNumber = $"SPLIT-{originalOrder.OrderNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var splitOrder = new OrderEntity
            {
                UserId = originalOrder.UserId,
                AddressId = dto.NewAddressId ?? originalOrder.AddressId,
                OrderNumber = splitOrderNumber,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = originalOrder.PaymentMethod,
                IsSplitOrder = true,
                ParentOrderId = originalOrder.Id
            };

            // Calculate split order totals
            decimal splitSubTotal = 0;
            var splitOrderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                var splitItem = new OrderItem
                {
                    OrderId = splitOrder.Id,
                    ProductId = originalItem.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = originalItem.UnitPrice,
                    TotalPrice = originalItem.UnitPrice * item.Quantity
                };
                splitSubTotal += splitItem.TotalPrice;
                splitOrderItems.Add(splitItem);

                // Update original order item quantity
                originalItem.Quantity -= item.Quantity;
                originalItem.TotalPrice = originalItem.UnitPrice * originalItem.Quantity;
            }

            splitOrder.SubTotal = splitSubTotal;
            splitOrder.ShippingCost = originalOrder.ShippingCost; // Can be recalculated
            splitOrder.Tax = splitSubTotal * (originalOrder.Tax / originalOrder.SubTotal);
            splitOrder.TotalAmount = splitOrder.SubTotal + splitOrder.ShippingCost + splitOrder.Tax;

            // ✅ PERFORMANCE: Memory'de Sum kullanılıyor - Ancak bu business logic için gerekli (order items zaten Include ile yüklenmiş)
            // Update original order totals
            originalOrder.SubTotal = originalOrder.OrderItems.Sum(oi => oi.TotalPrice);
            originalOrder.Tax = originalOrder.SubTotal * (originalOrder.Tax / (originalOrder.SubTotal + splitSubTotal));
            originalOrder.TotalAmount = originalOrder.SubTotal + originalOrder.ShippingCost + originalOrder.Tax;

            await _context.Orders.AddAsync(splitOrder);
            await _context.OrderItems.AddRangeAsync(splitOrderItems);
            await _unitOfWork.SaveChangesAsync();

            // Create OrderSplit record
            var orderSplit = new OrderSplit
            {
                OriginalOrderId = originalOrder.Id,
                SplitOrderId = splitOrder.Id,
                SplitReason = dto.SplitReason,
                NewAddressId = dto.NewAddressId,
                Status = OrderSplitStatus.Pending
            };

            await _context.Set<OrderSplit>().AddAsync(orderSplit);

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

            await _context.Set<OrderSplitItem>().AddRangeAsync(splitItemRecords);
            await _unitOfWork.SaveChangesAsync();

            // ✅ ARCHITECTURE: Transaction commit
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Sipariş başarıyla bölündü. OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}", orderId, splitOrder.Id);

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
                .FirstOrDefaultAsync(s => s.Id == orderSplit.Id);

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

    public async Task<OrderSplitDto?> GetSplitAsync(Guid splitId)
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
            .FirstOrDefaultAsync(s => s.Id == splitId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return split != null ? _mapper.Map<OrderSplitDto>(split) : null;
    }

    public async Task<IEnumerable<OrderSplitDto>> GetOrderSplitsAsync(Guid orderId)
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası foreach içinde MapToDto YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    public async Task<IEnumerable<OrderSplitDto>> GetSplitOrdersAsync(Guid splitOrderId)
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası foreach içinde MapToDto YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    public async Task<bool> CancelSplitAsync(Guid splitId)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var split = await _context.Set<OrderSplit>()
            .Include(s => s.SplitOrder)
            .Include(s => s.OriginalOrder)
            .FirstOrDefaultAsync(s => s.Id == splitId);

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
            .ToListAsync();

        foreach (var splitItem in splitItems)
        {
            var originalItem = splitItem.OriginalOrderItem;
            var splitOrderItem = splitItem.SplitOrderItem;
            
            originalItem.Quantity += splitItem.Quantity;
            originalItem.TotalPrice = originalItem.UnitPrice * originalItem.Quantity;
        }

        // ✅ PERFORMANCE: Memory'de Sum kullanılıyor - Ancak bu business logic için gerekli (order items zaten Include ile yüklenmiş)
        // Recalculate original order totals
        var originalOrder = split.OriginalOrder;
        originalOrder.SubTotal = originalOrder.OrderItems.Sum(oi => oi.TotalPrice);
        originalOrder.TotalAmount = originalOrder.SubTotal + originalOrder.ShippingCost + originalOrder.Tax;

        // Delete split order
        split.SplitOrder.IsDeleted = true;
        split.Status = OrderSplitStatus.Cancelled;
        split.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteSplitAsync(Guid splitId)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var split = await _context.Set<OrderSplit>()
            .FirstOrDefaultAsync(s => s.Id == splitId);

        if (split == null) return false;

        split.Status = OrderSplitStatus.Completed;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

}

