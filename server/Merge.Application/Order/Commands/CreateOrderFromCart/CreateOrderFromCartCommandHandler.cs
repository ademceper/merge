using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Marketing.Commands.ValidateCoupon;
using Merge.Application.Marketing.Queries.GetCouponByCode;
using Merge.Application.Cart.Commands.ClearCart;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CreateOrderFromCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateOrderFromCartCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMediator mediator, IMapper mapper, ILogger<CreateOrderFromCartCommandHandler> logger, IOptions<OrderSettings> orderSettings) : IRequestHandler<CreateOrderFromCartCommand, OrderDto>
{
    private readonly OrderSettings orderConfig = orderSettings.Value;

    public async Task<OrderDto> Handle(CreateOrderFromCartCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order from cart. UserId: {UserId}, AddressId: {AddressId}",
            request.UserId, request.AddressId);

        // ✅ CRITICAL: Transaction başlat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var cart = await context.Set<CartEntity>()
                .AsNoTracking()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart == null || cart.CartItems.Count == 0)
            {
                throw new BusinessException("Sepet boş.");
            }

            // ✅ PERFORMANCE: AsNoTracking for read-only query (check için)
            var address = await context.Set<AddressEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

            if (address == null)
            {
                throw new NotFoundException("Adres", request.AddressId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var order = OrderEntity.Create(request.UserId, request.AddressId, address);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan (AddItem)
            foreach (var cartItem in cart.CartItems)
            {
                order.AddItem(cartItem.Product, cartItem.Quantity);
                cartItem.Product.ReduceStock(cartItem.Quantity);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var shippingCost = new Money(CalculateShippingCost(order.SubTotal));
            order.SetShippingCost(shippingCost);

            var tax = new Money(CalculateTax(order.SubTotal));
            order.SetTax(tax);

            // Kupon indirimi uygula
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                await ApplyCouponAsync(order, cart, request.UserId, request.CouponCode, cancellationToken);
            }

            // ✅ BOLUM 1.5: Domain Event - OrderCreatedEvent'te TotalAmount güncelle
            // Order.Create'te event oluşturulurken TotalAmount 0 idi, şimdi gerçek değerle güncelle
            // Event'ler immutable olduğu için yeni event oluşturup eskisini kaldır
            var existingEvent = order.DomainEvents.OfType<OrderCreatedEvent>().FirstOrDefault();
            if (existingEvent != null)
            {
                order.RemoveDomainEvent(existingEvent);
                order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.UserId, order.TotalAmount));
            }

            await context.Set<OrderEntity>().AddAsync(order, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Kupon kullanımını kaydet
            if (!string.IsNullOrEmpty(request.CouponCode) && order.CouponDiscount.HasValue && order.CouponDiscount.Value > 0)
            {
                await RecordCouponUsageAsync(order, request.UserId, request.CouponCode, cancellationToken);
            }

            // Sepeti temizle
            // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
            await mediator.Send(new ClearCartCommand(request.UserId), cancellationToken);

            // ✅ CRITICAL: Commit all changes atomically
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            order = await context.Set<OrderEntity>()
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

            // ✅ ERROR HANDLING FIX: Null check instead of null-forgiving operator
            if (order == null)
            {
                logger.LogError("Order not found after creation. OrderId: {OrderId}", order?.Id);
                throw new InvalidOperationException("Order could not be retrieved after creation");
            }

            logger.LogInformation(
                "Order created successfully. OrderId: {OrderId}, OrderNumber: {OrderNumber}, UserId: {UserId}, TotalAmount: {TotalAmount}",
                order.Id, order.OrderNumber, request.UserId, order.TotalAmount);

            return mapper.Map<OrderDto>(order);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex,
                "Order creation failed. UserId: {UserId}, AddressId: {AddressId}, CouponCode: {CouponCode}",
                request.UserId, request.AddressId, request.CouponCode ?? "None");
            throw;
        }
    }

    private async Task ApplyCouponAsync(OrderEntity order, CartEntity cart, Guid userId, string couponCode, CancellationToken cancellationToken)
    {
        try
        {
            // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
            // ✅ FIX: ValidateCouponCommand List<Guid> bekliyor, ToList() gerekli
            var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
            
            var validateCommand = new ValidateCouponCommand(
                couponCode,
                order.SubTotal,
                userId,
                productIds);
            
            var couponDiscount = await mediator.Send(validateCommand, cancellationToken);

            if (couponDiscount > 0)
            {
                // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
                var getCouponQuery = new GetCouponByCodeQuery(couponCode);
                var couponDto = await mediator.Send(getCouponQuery, cancellationToken);
                
                if (couponDto != null)
                {
                    var coupon = await context.Set<Coupon>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == couponDto.Id, cancellationToken);

                    if (coupon != null)
                    {
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

    private async Task RecordCouponUsageAsync(OrderEntity order, Guid userId, string couponCode, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getCouponQuery = new GetCouponByCodeQuery(couponCode);
        var couponDto = await mediator.Send(getCouponQuery, cancellationToken);
        
        if (couponDto != null && order.CouponDiscount.HasValue)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var couponUsage = CouponUsage.Create(
                couponDto.Id,
                userId,
                order.Id,
                new Money(order.CouponDiscount.Value));
            await context.Set<CouponUsage>().AddAsync(couponUsage, cancellationToken);

            var couponEntity = await context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Id == couponDto.Id, cancellationToken);

            if (couponEntity != null)
            {
                couponEntity.IncrementUsage();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private decimal CalculateShippingCost(decimal subTotal)
    {
        return subTotal >= orderConfig.FreeShippingThreshold
            ? 0
            : orderConfig.DefaultShippingCost;
    }

    private decimal CalculateTax(decimal subTotal)
    {
        return subTotal * orderConfig.TaxRate;
    }
}
