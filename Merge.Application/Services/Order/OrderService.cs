using AutoMapper;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.Services;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;


namespace Merge.Application.Services.Order;

public class OrderService : IOrderService
{
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly ICartService _cartService;
    private readonly ICouponService _couponService;
    private readonly IEmailService? _emailService;
    private readonly ISmsService? _smsService;
    private readonly INotificationService? _notificationService;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly OrderSettings _orderSettings;

    public OrderService(
        IRepository<OrderEntity> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        ICartService cartService,
        ICouponService couponService,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger,
        IOptions<OrderSettings> orderSettings,
        IEmailService? emailService = null,
        ISmsService? smsService = null,
        INotificationService? notificationService = null)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _cartService = cartService;
        _couponService = couponService;
        _emailService = emailService;
        _smsService = smsService;
        _notificationService = notificationService;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _orderSettings = orderSettings.Value;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted check
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} orders for user {UserId}",
            orders.Count, userId);

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<PagedResult<OrderDto>> GetOrdersByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Pagination
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} orders (page {Page}) for user {UserId}",
            orders.Count, page, userId);

        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(Guid userId, Guid addressId, string? couponCode = null, CancellationToken cancellationToken = default)
    {
        // ✅ CRITICAL: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // ✅ PERFORMANCE: Removed manual !ci.IsDeleted and !c.IsDeleted checks
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            // ✅ PERFORMANCE: ToListAsync() sonrası Any() YASAK - List.Count kullan
            if (cart == null || cart.CartItems.Count == 0)
            {
                throw new BusinessException("Sepet boş.");
            }

            // ✅ PERFORMANCE: Address entity'sini çek (Create factory method için gerekli)
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);
            if (address == null)
            {
                throw new NotFoundException("Adres", addressId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var order = OrderEntity.Create(userId, addressId, address);

            // ✅ PERFORMANCE: Removed manual !ci.IsDeleted check (Global Query Filter)
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan (AddItem)
            foreach (var cartItem in cart.CartItems)
            {
                // AddItem içinde stock check yapılıyor
                order.AddItem(cartItem.Product, cartItem.Quantity);
                
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                // Stok güncelle (AddItem içinde kontrol ediliyor ama burada da güncelliyoruz)
                cartItem.Product.ReduceStock(cartItem.Quantity);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var shippingCost = new Money(CalculateShippingCost(order.SubTotal));
            order.SetShippingCost(shippingCost);
            
            var tax = new Money(CalculateTax(order.SubTotal));
            order.SetTax(tax);

            // Kupon indirimi uygula
            if (!string.IsNullOrEmpty(couponCode))
            {
                try
                {
                    var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
                    var couponDiscount = await _couponService.CalculateDiscountAsync(couponCode, order.SubTotal, userId, productIds);
                    
                    if (couponDiscount > 0)
                    {
                        // ✅ BOLUM 1.1: Rich Domain Model - Coupon entity gerekiyor
                        var couponDto = await _couponService.GetByCodeAsync(couponCode, cancellationToken);
                        if (couponDto != null)
                        {
                            // Coupon entity'sini context'ten çek
                            var coupon = await _context.Coupons
                                .FirstOrDefaultAsync(c => c.Id == couponDto.Id, cancellationToken);
                            
                            if (coupon != null)
                            {
                                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                                var discountMoney = new Money(couponDiscount);
                                order.ApplyCoupon(coupon, discountMoney);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new BusinessException($"Kupon uygulanamadı: {ex.Message}", ex);
                }
            }

            order = await _orderRepository.AddAsync(order);

            // Kupon kullanımını kaydet
            if (!string.IsNullOrEmpty(couponCode) && order.CouponDiscount.HasValue && order.CouponDiscount.Value > 0)
            {
                var couponDto = await _couponService.GetByCodeAsync(couponCode, cancellationToken);
                if (couponDto != null)
                {
                    var couponUsage = new CouponUsage
                    {
                        CouponId = couponDto.Id,
                        UserId = userId,
                        OrderId = order.Id,
                        DiscountAmount = order.CouponDiscount.Value
                    };
                    await _context.CouponUsages.AddAsync(couponUsage, cancellationToken);

                    // ✅ PERFORMANCE: FindAsync Global Query Filter'ı bypass eder - FirstOrDefaultAsync kullan
                    // Kupon kullanım sayısını artır
                    var couponEntity = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponDto.Id, cancellationToken);
                    if (couponEntity != null)
                    {
                        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                        couponEntity.IncrementUsage();
                    }
                }
            }

            // Sepeti temizle
            await _cartService.ClearCartAsync(userId, cancellationToken);

            // ✅ CRITICAL: Commit all changes atomically
            await _unitOfWork.CommitTransactionAsync();

            // Performance: Reload with all includes in one query instead of multiple LoadAsync calls
            order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

            _logger.LogInformation(
                "Order created successfully. OrderId: {OrderId}, OrderNumber: {OrderNumber}, UserId: {UserId}, TotalAmount: {TotalAmount}",
                order!.Id, order.OrderNumber, userId, order.TotalAmount);

            return _mapper.Map<OrderDto>(order);
        }
        catch (Exception ex)
        {
            // ✅ CRITICAL: Rollback on any error
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex,
                "Order creation failed. UserId: {UserId}, AddressId: {AddressId}, CouponCode: {CouponCode}",
                userId, addressId, couponCode ?? "None");
            throw;
        }
    }

    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        var oldStatus = order.Status;
        
        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        order.TransitionTo(status);

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Single query with all includes instead of multiple LoadAsync calls
        order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        _logger.LogInformation(
            "Order status updated. OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            orderId, oldStatus, status);

        return _mapper.Map<OrderDto>(order!);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter)
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            return false;
        }

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
        {
            throw new BusinessException("Bu sipariş iptal edilemez.");
        }

        // ✅ CRITICAL: Transaction for atomic stock restoration
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Stokları geri ekle
            foreach (var item in order.OrderItems)
            {
                item.Product.IncreaseStock(item.Quantity);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            order.Cancel();
            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Order cancelled successfully. OrderId: {OrderId}, ItemsRestored: {ItemCount}",
                orderId, order.OrderItems.Count);

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Order cancellation failed. OrderId: {OrderId}", orderId);
            throw;
        }
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    private decimal CalculateShippingCost(decimal subTotal)
    {
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        return subTotal >= _orderSettings.FreeShippingThreshold 
            ? 0 
            : _orderSettings.DefaultShippingCost;
    }

    private decimal CalculateTax(decimal subTotal)
    {
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        return subTotal * _orderSettings.TaxRate;
    }

    public async Task<OrderDto> ReorderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted check
        var originalOrder = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);

        if (originalOrder == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        var addedItems = 0;
        var skippedItems = 0;

        // Sepete ekle
        foreach (var orderItem in originalOrder.OrderItems)
        {
            // Ürün hala aktif ve stokta var mı kontrol et
            if (orderItem.Product.IsActive && orderItem.Product.StockQuantity > 0)
            {
                try
                {
                    await _cartService.AddItemToCartAsync(userId, orderItem.ProductId, orderItem.Quantity, cancellationToken);
                    addedItems++;
                }
                catch
                {
                    // Ürün sepete eklenemezse devam et
                    skippedItems++;
                }
            }
            else
            {
                skippedItems++;
            }
        }

        _logger.LogInformation(
            "Reorder completed. OriginalOrderId: {OrderId}, AddedItems: {AddedItems}, SkippedItems: {SkippedItems}",
            orderId, addedItems, skippedItems);

        // Yeni sipariş oluştur (kupon ve adres bilgilerini kullan)
        return await CreateOrderFromCartAsync(userId, originalOrder.AddressId, null, cancellationToken);
    }

    public async Task<byte[]> ExportOrdersToCsvAsync(OrderExportDto exportDto, CancellationToken cancellationToken = default)
    {
        var orders = await GetOrdersForExportAsync(exportDto, cancellationToken);

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("OrderNumber,UserId,SubTotal,ShippingCost,Tax,TotalAmount,Status,PaymentStatus,CreatedAt");

        foreach (var order in orders)
        {
            csv.AppendLine($"\"{order.OrderNumber}\"," +
                          $"\"{order.UserId}\"," +
                          $"{order.SubTotal}," +
                          $"{order.ShippingCost}," +
                          $"{order.Tax}," +
                          $"{order.TotalAmount}," +
                          $"\"{order.Status}\"," +
                          $"\"{order.PaymentStatus}\"," +
                          $"\"{order.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportOrdersToJsonAsync(OrderExportDto exportDto, CancellationToken cancellationToken = default)
    {
        var orders = await GetOrdersForExportAsync(exportDto, cancellationToken);

        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - Ancak bu export için DTO'dan gelen list üzerinde işlem yapılıyor
        // Not: Bu export işlemi için minimal bir işlem ve business logic için gerekli
        var exportData = orders.Select(o => new
        {
            o.OrderNumber,
            o.UserId,
            o.SubTotal,
            o.ShippingCost,
            o.Tax,
            o.TotalAmount,
            o.Status,
            o.PaymentStatus,
            o.CreatedAt,
            OrderItems = exportDto.IncludeOrderItems ? o.OrderItems.Select(oi => new
            {
                oi.ProductName,
                oi.Quantity,
                Price = oi.Price,
                oi.TotalPrice
            }) : null,
            Address = exportDto.IncludeAddress ? new
            {
                o.Address.AddressLine1,
                o.Address.AddressLine2,
                o.Address.City,
                o.Address.Country,
                o.Address.PostalCode
            } : null
        }).ToList();

        var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public async Task<byte[]> ExportOrdersToExcelAsync(OrderExportDto exportDto, CancellationToken cancellationToken = default)
    {
        // For Excel export, we'll use CSV format as a simple alternative
        // In production, you might want to use a library like EPPlus or ClosedXML
        return await ExportOrdersToCsvAsync(exportDto, cancellationToken);
    }

    private async Task<List<OrderDto>> GetOrdersForExportAsync(OrderExportDto exportDto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted check (Global Query Filter)
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .AsQueryable();

        if (exportDto.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= exportDto.StartDate.Value);
        }

        if (exportDto.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= exportDto.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(exportDto.Status))
        {
            var statusEnum = Enum.Parse<OrderStatus>(exportDto.Status);
            query = query.Where(o => o.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(exportDto.PaymentStatus))
        {
            var paymentStatusEnum = Enum.Parse<PaymentStatus>(exportDto.PaymentStatus);
            query = query.Where(o => o.PaymentStatus == paymentStatusEnum);
        }

        if (exportDto.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == exportDto.UserId.Value);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Orders exported. Count: {Count}, StartDate: {StartDate}, EndDate: {EndDate}",
            orders.Count, exportDto.StartDate, exportDto.EndDate);

        return _mapper.Map<List<OrderDto>>(orders);
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape quotes
        return value.Replace("\"", "\"\"");
    }
}

