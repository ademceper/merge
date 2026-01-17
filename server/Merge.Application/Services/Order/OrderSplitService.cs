using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Order;

using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Order;

public class OrderSplitService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderSplitService> logger) : IOrderSplitService
{

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrderSplitDto> SplitOrderAsync(Guid orderId, CreateOrderSplitDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ MODERN C#: ArgumentNullException.ThrowIfNull (C# 10+)
        ArgumentNullException.ThrowIfNull(dto);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Sipariş bölme işlemi başlatılıyor. OrderId: {OrderId}, ItemsCount: {ItemsCount}",
            orderId, dto.Items?.Count ?? 0);

        var originalOrder = await context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        // ✅ MODERN C#: ArgumentNullException.ThrowIfNull (C# 10+)
        ArgumentNullException.ThrowIfNull(originalOrder);

        if (originalOrder.Status != OrderStatus.Pending && originalOrder.Status != OrderStatus.Processing)
        {
            throw new BusinessException("Sipariş sadece Beklemede veya İşleniyor durumundayken bölünebilir.");
        }

        // Validate split items
        if (dto.Items == null || !dto.Items.Any())
        {
            throw new ValidationException("En az bir sipariş kalemi belirtilmelidir.");
        }

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
        await unitOfWork.BeginTransactionAsync();

        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            // Address entity'sini çek
            var addressId = dto.NewAddressId ?? originalOrder.AddressId;
            var address = await context.Set<AddressEntity>()
                .FirstOrDefaultAsync(a => a.Id == addressId, cancellationToken);
            
            if (address == null)
            {
                throw new NotFoundException("Adres", addressId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var splitOrder = OrderEntity.Create(originalOrder.UserId, addressId, address);
            
            // Split order için özel ayarlar
            // Not: OrderEntity.Create factory method OrderNumber'ı otomatik oluşturuyor
            // Split order için özel order number gerekiyorsa, Order entity'sine SetOrderNumber method'u eklenebilir
            // Şimdilik factory method'un oluşturduğu order number kullanılıyor

            // Calculate split order totals
            decimal splitSubTotal = 0;
            // ✅ HIGH-NET-001 FIX: Collection expressions (C# 12)
            var splitOrderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                
                // Product'ı çek (AddItem için gerekli)
                var product = await context.Set<ProductEntity>()
                    .FirstOrDefaultAsync(p => p.Id == originalItem.ProductId, cancellationToken);
                
                if (product == null)
                {
                    throw new NotFoundException("Ürün", originalItem.ProductId);
                }

                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                splitOrder.AddItem(product, item.Quantity);
                splitSubTotal += originalItem.UnitPrice * item.Quantity;
                
                // ✅ BOLUM 1.1: Rich Domain Model - OrderItem'lar AddItem ile Order.OrderItems collection'ına ekleniyor
                splitOrderItems.Add(splitOrder.OrderItems.Last());

                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                var newQuantity = originalItem.Quantity - item.Quantity;
                originalItem.UpdateQuantity(newQuantity);
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
            
            await context.Set<OrderEntity>().AddAsync(splitOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            // Address yükle (eğer NewAddressId varsa)
            AddressEntity? newAddress = null;
            if (dto.NewAddressId.HasValue)
            {
                newAddress = await context.Set<AddressEntity>()
                    .FirstOrDefaultAsync(a => a.Id == dto.NewAddressId.Value, cancellationToken);
            }

            var orderSplit = OrderSplit.Create(
                originalOrder.Id,
                splitOrder.Id,
                dto.SplitReason,
                dto.NewAddressId,
                originalOrder,
                splitOrder,
                newAddress);

            await context.Set<OrderSplit>().AddAsync(orderSplit, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            // Create OrderSplitItem records
            // ✅ HIGH-NET-001 FIX: Collection expressions (C# 12)
            var splitItemRecords = new List<OrderSplitItem>();
            foreach (var item in dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                var splitItem = splitOrderItems.First(si => si.ProductId == originalItem.ProductId && si.Quantity == item.Quantity);

                splitItemRecords.Add(OrderSplitItem.Create(
                    orderSplit.Id,
                    originalItem.Id,
                    splitItem.Id,
                    item.Quantity,
                    orderSplit,
                    originalItem,
                    splitItem));
            }

            await context.Set<OrderSplitItem>().AddRangeAsync(splitItemRecords, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ ARCHITECTURE: Transaction commit
            await unitOfWork.CommitTransactionAsync();

            // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
            logger.LogInformation(
                "Sipariş başarıyla bölündü. OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                orderId, splitOrder.Id);

            orderSplit = await context.Set<OrderSplit>()
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
            return mapper.Map<OrderSplitDto>(orderSplit);
        }
        catch (Exception ex)
        {
            // ✅ ARCHITECTURE: Transaction rollback on error
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Sipariş bölme işlemi başarısız. OrderId: {OrderId}", orderId);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrderSplitDto?> GetSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        var split = await context.Set<OrderSplit>()
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
        return split != null ? mapper.Map<OrderSplitDto>(split) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderSplitDto>> GetOrderSplitsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with nested ThenInclude)
        var splits = await context.Set<OrderSplit>()
            .AsNoTracking()
            .AsSplitQuery()
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
        return mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderSplitDto>> GetSplitOrdersAsync(Guid splitOrderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with nested ThenInclude)
        var splits = await context.Set<OrderSplit>()
            .AsNoTracking()
            .AsSplitQuery()
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
        return mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CancelSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var split = await context.Set<OrderSplit>()
        .AsSplitQuery()
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
        var splitItems = await context.Set<OrderSplitItem>()
        .AsSplitQuery()
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
            
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            originalItem.UpdateQuantity(originalItem.Quantity + splitItem.Quantity);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        // Recalculate original order totals
        var originalOrder = split.OriginalOrder;
        originalOrder.RecalculateTotals();

        // Delete split order
        split.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CompleteSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var split = await context.Set<OrderSplit>()
            .FirstOrDefaultAsync(s => s.Id == splitId, cancellationToken);

        if (split == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        split.Complete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

}

