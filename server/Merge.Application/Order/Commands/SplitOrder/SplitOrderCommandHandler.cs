using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.SplitOrder;

public class SplitOrderCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SplitOrderCommandHandler> logger) : IRequestHandler<SplitOrderCommand, OrderSplitDto>
{

    public async Task<OrderSplitDto> Handle(SplitOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto is null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        logger.LogInformation(
            "Sipariş bölme işlemi başlatılıyor. OrderId: {OrderId}, ItemsCount: {ItemsCount}",
            request.OrderId, request.Dto.Items?.Count ?? 0);

        var originalOrder = await context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (originalOrder is null)
        {
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        if (originalOrder.Status != OrderStatus.Pending && originalOrder.Status != OrderStatus.Processing)
        {
            throw new BusinessException("Sipariş sadece Beklemede veya İşleniyor durumundayken bölünebilir.");
        }

        if (request.Dto.Items is null || request.Dto.Items.Count == 0)
        {
            throw new ValidationException("En az bir sipariş kalemi belirtilmelidir.");
        }

        var totalSplitQuantity = 0;
        foreach (var item in request.Dto.Items)
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

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var addressId = request.Dto.NewAddressId ?? originalOrder.AddressId;
            var address = await context.Set<AddressEntity>()
                .FirstOrDefaultAsync(a => a.Id == addressId, cancellationToken);
            
            if (address is null)
            {
                throw new NotFoundException("Adres", addressId);
            }

            var splitOrder = OrderEntity.Create(originalOrder.UserId, addressId, address);
            
            decimal splitSubTotal = 0;
            List<OrderItem> splitOrderItems = [];

            foreach (var item in request.Dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                var product = await context.Set<ProductEntity>()
                    .FirstOrDefaultAsync(p => p.Id == originalItem.ProductId, cancellationToken);
                
                if (product is null)
                {
                    throw new NotFoundException("Ürün", originalItem.ProductId);
                }

                var itemCountBefore = splitOrder.OrderItems.Count;

                splitOrder.AddItem(product, item.Quantity);
                splitSubTotal += originalItem.UnitPrice * item.Quantity;

                // Update original order item quantity
                var newQuantity = originalItem.Quantity - item.Quantity;
                if (newQuantity <= 0)
                {
                    throw new ValidationException($"Sipariş kalemi {item.OrderItemId} için miktar 0 veya negatif olamaz.");
                }
                originalItem.UpdateQuantity(newQuantity);
                
                // AddItem sonrası collection'a yeni item eklendi, son index'teki item'ı al
                var addedItem = splitOrder.OrderItems
                    .Skip(itemCountBefore)
                    .FirstOrDefault();
                if (addedItem is not null)
                {
                    splitOrderItems.Add(addedItem);
                }
            }

            var shippingCost = new Money(originalOrder.ShippingCost);
            splitOrder.SetShippingCost(shippingCost);
            
            // Tax hesaplama - original order'ın tax oranını kullan
            var taxRate = originalOrder.SubTotal > 0 ? originalOrder.Tax / originalOrder.SubTotal : 0;
            var tax = new Money(splitSubTotal * taxRate);
            splitOrder.SetTax(tax);

            originalOrder.RecalculateTotals();
            
            await context.Set<OrderEntity>().AddAsync(splitOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var newAddress = request.Dto.NewAddressId.HasValue
                ? await context.Set<AddressEntity>()
                    .FirstOrDefaultAsync(a => a.Id == request.Dto.NewAddressId.Value, cancellationToken)
                : null;

            var orderSplit = OrderSplit.Create(
                originalOrder.Id,
                splitOrder.Id,
                request.Dto.SplitReason,
                request.Dto.NewAddressId,
                originalOrder,
                splitOrder,
                newAddress);

            await context.Set<OrderSplit>().AddAsync(orderSplit, cancellationToken);

            List<OrderSplitItem> splitItemRecords = [];
            var splitItemIndex = 0;
            foreach (var item in request.Dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                
                if (splitItemIndex >= splitOrderItems.Count)
                {
                    throw new NotFoundException("Split order item", originalItem.Id);
                }
                
                var splitItem = splitOrderItems[splitItemIndex];
                
                // Validation: Split item'ın product ve quantity'si eşleşmeli
                if (splitItem.ProductId != originalItem.ProductId || splitItem.Quantity != item.Quantity)
                {
                    throw new BusinessException($"Split order item validation failed for OrderItemId: {item.OrderItemId}");
                }

                var splitItemRecord = OrderSplitItem.Create(
                    orderSplit.Id,
                    originalItem.Id,
                    splitItem.Id,
                    item.Quantity,
                    orderSplit,
                    originalItem,
                    splitItem);
                
                splitItemRecords.Add(splitItemRecord);
                
                splitItemIndex++;
            }

            await context.Set<OrderSplitItem>().AddRangeAsync(splitItemRecords, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Sipariş başarıyla bölündü. OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                request.OrderId, splitOrder.Id);

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

            return mapper.Map<OrderSplitDto>(orderSplit!);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Sipariş bölme işlemi başarısız. OrderId: {OrderId}", request.OrderId);
            throw;
        }
    }
}
