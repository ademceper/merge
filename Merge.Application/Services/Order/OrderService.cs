using AutoMapper;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.Services;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;
using Microsoft.Extensions.Logging;


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

    public OrderService(
        IRepository<OrderEntity> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        ICartService cartService,
        ICouponService couponService,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger,
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
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId)
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
            .ToListAsync();

        _logger.LogInformation(
            "Retrieved {Count} orders for user {UserId}",
            orders.Count, userId);

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(Guid userId, Guid addressId, string? couponCode = null)
    {
        // ✅ CRITICAL: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // ✅ PERFORMANCE: Removed manual !ci.IsDeleted and !c.IsDeleted checks
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // ✅ PERFORMANCE: ToListAsync() sonrası Any() YASAK - List.Count kullan
            if (cart == null || cart.CartItems.Count == 0)
            {
                throw new BusinessException("Sepet boş.");
            }

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            if (address == null)
            {
                throw new NotFoundException("Adres", addressId);
            }

            var order = new OrderEntity
            {
                UserId = userId,
                AddressId = addressId,
                OrderNumber = GenerateOrderNumber(),
                Status = "Pending",
                PaymentStatus = "Pending"
            };

            // ✅ PERFORMANCE: Removed manual !ci.IsDeleted check (Global Query Filter)
            decimal subTotal = 0;
            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Product.StockQuantity < cartItem.Quantity)
                {
                    throw new BusinessException($"{cartItem.Product.Name} için yeterli stok yok.");
                }

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price,
                    TotalPrice = cartItem.Quantity * cartItem.Price
                };

                order.OrderItems.Add(orderItem);
                subTotal += orderItem.TotalPrice;

                // Stok güncelle
                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            order.SubTotal = subTotal;
            order.ShippingCost = CalculateShippingCost(subTotal);
            order.Tax = CalculateTax(subTotal);

            // Kupon indirimi uygula
            decimal couponDiscount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                try
                {
                    // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - Database'de Select yap
                    // Not: cart.CartItems zaten Include ile yüklenmiş, bu yüzden memory'de Select yapıyoruz
                    // Ancak bu minimal bir işlem ve business logic için gerekli
                    var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
                    couponDiscount = await _couponService.CalculateDiscountAsync(couponCode, subTotal, userId, productIds);
                    order.CouponDiscount = couponDiscount;
                }
                catch (Exception ex)
                {
                    throw new BusinessException($"Kupon uygulanamadı: {ex.Message}", ex);
                }
            }

            order.TotalAmount = order.SubTotal + order.ShippingCost + order.Tax - couponDiscount;

            order = await _orderRepository.AddAsync(order);

            // Kupon kullanımını kaydet
            if (!string.IsNullOrEmpty(couponCode) && couponDiscount > 0)
            {
                var coupon = await _couponService.GetByCodeAsync(couponCode);
                if (coupon != null)
                {
                    var couponUsage = new CouponUsage
                    {
                        CouponId = coupon.Id,
                        UserId = userId,
                        OrderId = order.Id,
                        DiscountAmount = couponDiscount
                    };
                    await _context.CouponUsages.AddAsync(couponUsage);

                    // ✅ PERFORMANCE: FindAsync Global Query Filter'ı bypass eder - FirstOrDefaultAsync kullan
                    // Kupon kullanım sayısını artır
                    var couponEntity = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == coupon.Id);
                    if (couponEntity != null)
                    {
                        couponEntity.UsedCount++;
                    }
                }
            }

            // Sepeti temizle
            await _cartService.ClearCartAsync(userId);

            // ✅ CRITICAL: Commit all changes atomically
            await _unitOfWork.CommitTransactionAsync();

            // Performance: Reload with all includes in one query instead of multiple LoadAsync calls
            order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

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

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        var oldStatus = order.Status;
        order.Status = status;
        if (status == "Shipped")
        {
            order.ShippedDate = DateTime.UtcNow;
        }
        else if (status == "Delivered")
        {
            order.DeliveredDate = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Single query with all includes instead of multiple LoadAsync calls
        order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        _logger.LogInformation(
            "Order status updated. OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            orderId, oldStatus, status);

        return _mapper.Map<OrderDto>(order!);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter)
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return false;
        }

        if (order.Status == "Delivered" || order.Status == "Shipped")
        {
            throw new BusinessException("Bu sipariş iptal edilemez.");
        }

        // ✅ CRITICAL: Transaction for atomic stock restoration
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Stokları geri ekle
            foreach (var item in order.OrderItems)
            {
                item.Product.StockQuantity += item.Quantity;
            }

            order.Status = "Cancelled";
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
        // Ücretsiz kargo eşiği: 500 TL
        return subTotal >= 500 ? 0 : 50;
    }

    private decimal CalculateTax(decimal subTotal)
    {
        // KDV %20
        return subTotal * 0.20m;
    }

    public async Task<OrderDto> ReorderAsync(Guid orderId, Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted check
        var originalOrder = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

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
                    await _cartService.AddItemToCartAsync(userId, orderItem.ProductId, orderItem.Quantity);
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
        return await CreateOrderFromCartAsync(userId, originalOrder.AddressId, null);
    }

    public async Task<byte[]> ExportOrdersToCsvAsync(OrderExportDto exportDto)
    {
        var orders = await GetOrdersForExportAsync(exportDto);

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

    public async Task<byte[]> ExportOrdersToJsonAsync(OrderExportDto exportDto)
    {
        var orders = await GetOrdersForExportAsync(exportDto);

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

    public async Task<byte[]> ExportOrdersToExcelAsync(OrderExportDto exportDto)
    {
        // For Excel export, we'll use CSV format as a simple alternative
        // In production, you might want to use a library like EPPlus or ClosedXML
        return await ExportOrdersToCsvAsync(exportDto);
    }

    private async Task<List<OrderDto>> GetOrdersForExportAsync(OrderExportDto exportDto)
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
            query = query.Where(o => o.Status == exportDto.Status);
        }

        if (!string.IsNullOrEmpty(exportDto.PaymentStatus))
        {
            query = query.Where(o => o.PaymentStatus == exportDto.PaymentStatus);
        }

        if (exportDto.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == exportDto.UserId.Value);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

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

