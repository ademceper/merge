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

    public async Task<OrderSplitDto> SplitOrderAsync(Guid orderId, CreateOrderSplitDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        logger.LogInformation(
            "Sipariş bölme işlemi başlatılıyor. OrderId: {OrderId}, ItemsCount: {ItemsCount}",
            orderId, dto.Items?.Count ?? 0);

        var originalOrder = await context.Set<OrderEntity>()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        ArgumentNullException.ThrowIfNull(originalOrder);

        if (originalOrder.Status != OrderStatus.Pending && originalOrder.Status != OrderStatus.Processing)
        {
            throw new BusinessException("Sipariş sadece Beklemede veya İşleniyor durumundayken bölünebilir.");
        }

        // Validate split items
        if (dto.Items is null || !dto.Items.Any())
        {
            throw new ValidationException("En az bir sipariş kalemi belirtilmelidir.");
        }

        var totalSplitQuantity = 0;
        foreach (var item in dto.Items)
        {
            var orderItem = originalOrder.OrderItems.FirstOrDefault(oi => oi.Id == item.OrderItemId);
            if (orderItem is null)
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

        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Address entity'sini çek
            var addressId = dto.NewAddressId ?? originalOrder.AddressId;
            var address = await context.Set<AddressEntity>()
                .FirstOrDefaultAsync(a => a.Id == addressId, cancellationToken);
            
            if (address is null)
            {
                throw new NotFoundException("Adres", addressId);
            }

            var splitOrder = OrderEntity.Create(originalOrder.UserId, addressId, address);
            
            // Split order için özel ayarlar
            // Not: OrderEntity.Create factory method OrderNumber'ı otomatik oluşturuyor
            // Split order için özel order number gerekiyorsa, Order entity'sine SetOrderNumber method'u eklenebilir
            // Şimdilik factory method'un oluşturduğu order number kullanılıyor

            // Calculate split order totals
            decimal splitSubTotal = 0;
            List<OrderItem> splitOrderItems = [];

            foreach (var item in dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                
                // Product'ı çek (AddItem için gerekli)
                var product = await context.Set<ProductEntity>()
                    .FirstOrDefaultAsync(p => p.Id == originalItem.ProductId, cancellationToken);
                
                if (product is null)
                {
                    throw new NotFoundException("Ürün", originalItem.ProductId);
                }

                splitOrder.AddItem(product, item.Quantity);
                splitSubTotal += originalItem.UnitPrice * item.Quantity;
                
                splitOrderItems.Add(splitOrder.OrderItems.Last());

                var newQuantity = originalItem.Quantity - item.Quantity;
                originalItem.UpdateQuantity(newQuantity);
            }

            var shippingCost = new Money(originalOrder.ShippingCost);
            splitOrder.SetShippingCost(shippingCost);
            
            var tax = new Money(splitSubTotal * (originalOrder.Tax / originalOrder.SubTotal));
            splitOrder.SetTax(tax);

            // Update original order totals - Domain method'lar kullanılamıyor çünkü Order entity'sinde RecalculateTotals private
            // Bu durumda service layer'da manuel hesaplama yapılıyor (Order entity'sine public RecalculateTotals eklenebilir)
            // Şimdilik manuel hesaplama yapılıyor
            
            await context.Set<OrderEntity>().AddAsync(splitOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

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

            // Create OrderSplitItem records
            List<OrderSplitItem> splitItemRecords = [];
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

            await unitOfWork.CommitTransactionAsync();

            logger.LogInformation(
                "Sipariş başarıyla bölündü. OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                orderId, splitOrder.Id);

            orderSplit = await context.Set<OrderSplit>()
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
                .FirstOrDefaultAsync(s => s.Id == orderSplit.Id, cancellationToken);

            return mapper.Map<OrderSplitDto>(orderSplit);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Sipariş bölme işlemi başarısız. OrderId: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<OrderSplitDto?> GetSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        var split = await context.Set<OrderSplit>()
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
            .FirstOrDefaultAsync(s => s.Id == splitId, cancellationToken);

        return split is not null ? mapper.Map<OrderSplitDto>(split) : null;
    }

    public async Task<IEnumerable<OrderSplitDto>> GetOrderSplitsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
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

        return mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    public async Task<IEnumerable<OrderSplitDto>> GetSplitOrdersAsync(Guid splitOrderId, CancellationToken cancellationToken = default)
    {
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

        return mapper.Map<IEnumerable<OrderSplitDto>>(splits);
    }

    public async Task<bool> CancelSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        var split = await context.Set<OrderSplit>()
        .AsSplitQuery()
            .Include(s => s.SplitOrder)
            .Include(s => s.OriginalOrder)
            .FirstOrDefaultAsync(s => s.Id == splitId, cancellationToken);

        if (split is null) return false;

        if (split.SplitOrder.Status != OrderStatus.Pending)
        {
            throw new BusinessException("Beklemede durumunda olmayan bölünmüş sipariş iptal edilemez.");
        }

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
            
            originalItem.UpdateQuantity(originalItem.Quantity + splitItem.Quantity);
        }

        // Recalculate original order totals
        var originalOrder = split.OriginalOrder;
        originalOrder.RecalculateTotals();

        // Delete split order
        split.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CompleteSplitAsync(Guid splitId, CancellationToken cancellationToken = default)
    {
        var split = await context.Set<OrderSplit>()
            .FirstOrDefaultAsync(s => s.Id == splitId, cancellationToken);

        if (split is null) return false;

        split.Complete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

}

