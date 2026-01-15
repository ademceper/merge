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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SplitOrderCommandHandler : IRequestHandler<SplitOrderCommand, OrderSplitDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SplitOrderCommandHandler> _logger;

    public SplitOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SplitOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderSplitDto> Handle(SplitOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto == null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        _logger.LogInformation(
            "Sipariş bölme işlemi başlatılıyor. OrderId: {OrderId}, ItemsCount: {ItemsCount}",
            request.OrderId, request.Dto.Items?.Count ?? 0);

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var originalOrder = await _context.Set<OrderEntity>()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (originalOrder == null)
        {
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        if (originalOrder.Status != OrderStatus.Pending && originalOrder.Status != OrderStatus.Processing)
        {
            throw new BusinessException("Sipariş sadece Beklemede veya İşleniyor durumundayken bölünebilir.");
        }

        // ✅ PERFORMANCE: Memory'de kontrol (DTO'dan geldiği için database query gerekmez)
        if (request.Dto.Items == null || request.Dto.Items.Count == 0)
        {
            throw new ValidationException("En az bir sipariş kalemi belirtilmelidir.");
        }

        var totalSplitQuantity = 0;
        foreach (var item in request.Dto.Items)
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

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var addressId = request.Dto.NewAddressId ?? originalOrder.AddressId;
            var address = await _context.Set<AddressEntity>()
                .FirstOrDefaultAsync(a => a.Id == addressId, cancellationToken);
            
            if (address == null)
            {
                throw new NotFoundException("Adres", addressId);
            }

            var splitOrder = OrderEntity.Create(originalOrder.UserId, addressId, address);
            
            decimal splitSubTotal = 0;
            var splitOrderItems = new List<OrderItem>();

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan (AddItem)
            foreach (var item in request.Dto.Items)
            {
                var originalItem = originalOrder.OrderItems.First(oi => oi.Id == item.OrderItemId);
                var product = await _context.Set<ProductEntity>()
                    .FirstOrDefaultAsync(p => p.Id == originalItem.ProductId, cancellationToken);
                
                if (product == null)
                {
                    throw new NotFoundException("Ürün", originalItem.ProductId);
                }

                // ✅ PERFORMANCE: AddItem öncesi item count'u kaydet
                var itemCountBefore = splitOrder.OrderItems.Count;

                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                splitOrder.AddItem(product, item.Quantity);
                splitSubTotal += originalItem.UnitPrice * item.Quantity;

                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan (UpdateQuantity)
                // Update original order item quantity
                var newQuantity = originalItem.Quantity - item.Quantity;
                if (newQuantity <= 0)
                {
                    throw new ValidationException($"Sipariş kalemi {item.OrderItemId} için miktar 0 veya negatif olamaz.");
                }
                originalItem.UpdateQuantity(newQuantity);
                
                // ✅ PERFORMANCE: AddItem sonrası eklenen item'ı al (index kullan)
                // AddItem sonrası collection'a yeni item eklendi, son index'teki item'ı al
                var addedItem = splitOrder.OrderItems
                    .Skip(itemCountBefore)
                    .FirstOrDefault();
                if (addedItem != null)
                {
                    splitOrderItems.Add(addedItem);
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var shippingCost = new Money(originalOrder.ShippingCost);
            splitOrder.SetShippingCost(shippingCost);
            
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Tax hesaplama - original order'ın tax oranını kullan
            var taxRate = originalOrder.SubTotal > 0 ? originalOrder.Tax / originalOrder.SubTotal : 0;
            var tax = new Money(splitSubTotal * taxRate);
            splitOrder.SetTax(tax);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            originalOrder.RecalculateTotals();
            
            await _context.Set<OrderEntity>().AddAsync(splitOrder, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var newAddress = request.Dto.NewAddressId.HasValue
                ? await _context.Set<AddressEntity>()
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

            await _context.Set<OrderSplit>().AddAsync(orderSplit, cancellationToken);

            var splitItemRecords = new List<OrderSplitItem>();
            // ✅ PERFORMANCE: splitOrderItems'ı index'e göre eşleştir (sıralı olduğu için)
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

                // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
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

            await _context.Set<OrderSplitItem>().AddRangeAsync(splitItemRecords, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Sipariş başarıyla bölündü. OriginalOrderId: {OriginalOrderId}, SplitOrderId: {SplitOrderId}",
                request.OrderId, splitOrder.Id);

            // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
            orderSplit = await _context.Set<OrderSplit>()
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

            return _mapper.Map<OrderSplitDto>(orderSplit!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Sipariş bölme işlemi başarısız. OrderId: {OrderId}", request.OrderId);
            throw;
        }
    }
}
