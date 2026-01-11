using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Marketing.Commands.ValidateCoupon;
using Merge.Application.Marketing.Queries.GetCouponByCode;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using OrderEntity = Merge.Domain.Entities.Order;
using CartEntity = Merge.Domain.Entities.Cart;

namespace Merge.Application.Order.Commands.CreateOrderFromCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateOrderFromCartCommandHandler : IRequestHandler<CreateOrderFromCartCommand, OrderDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartService _cartService;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderFromCartCommandHandler> _logger;
    private readonly OrderSettings _orderSettings;

    public CreateOrderFromCartCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICartService cartService,
        IMediator mediator,
        IMapper mapper,
        ILogger<CreateOrderFromCartCommandHandler> logger,
        IOptions<OrderSettings> orderSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cartService = cartService;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
        _orderSettings = orderSettings.Value;
    }

    public async Task<OrderDto> Handle(CreateOrderFromCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order from cart. UserId: {UserId}, AddressId: {AddressId}",
            request.UserId, request.AddressId);

        // ✅ CRITICAL: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ PERFORMANCE: AsNoTracking for read-only query (check için)
            var cart = await _context.Set<CartEntity>()
                .AsNoTracking()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart == null || cart.CartItems.Count == 0)
            {
                throw new BusinessException("Sepet boş.");
            }

            // ✅ PERFORMANCE: AsNoTracking for read-only query (check için)
            var address = await _context.Set<Address>()
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

            await _context.Set<OrderEntity>().AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Kupon kullanımını kaydet
            if (!string.IsNullOrEmpty(request.CouponCode) && order.CouponDiscount.HasValue && order.CouponDiscount.Value > 0)
            {
                await RecordCouponUsageAsync(order, request.UserId, request.CouponCode, cancellationToken);
            }

            // Sepeti temizle
            await _cartService.ClearCartAsync(request.UserId, cancellationToken);

            // ✅ CRITICAL: Commit all changes atomically
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Performance: Reload with all includes in one query
            order = await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

            _logger.LogInformation(
                "Order created successfully. OrderId: {OrderId}, OrderNumber: {OrderNumber}, UserId: {UserId}, TotalAmount: {TotalAmount}",
                order!.Id, order.OrderNumber, request.UserId, order.TotalAmount);

            return _mapper.Map<OrderDto>(order);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex,
                "Order creation failed. UserId: {UserId}, AddressId: {AddressId}, CouponCode: {CouponCode}",
                request.UserId, request.AddressId, request.CouponCode ?? "None");
            throw;
        }
    }

    private async Task ApplyCouponAsync(OrderEntity order, CartEntity cart, Guid userId, string couponCode, CancellationToken cancellationToken)
    {
        try
        {
            var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
            
            // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
            var validateCommand = new ValidateCouponCommand(
                couponCode,
                order.SubTotal,
                userId,
                productIds);
            
            var couponDiscount = await _mediator.Send(validateCommand, cancellationToken);

            if (couponDiscount > 0)
            {
                // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
                var getCouponQuery = new GetCouponByCodeQuery(couponCode);
                var couponDto = await _mediator.Send(getCouponQuery, cancellationToken);
                
                if (couponDto != null)
                {
                    var coupon = await _context.Set<Coupon>()
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
        var couponDto = await _mediator.Send(getCouponQuery, cancellationToken);
        
        if (couponDto != null && order.CouponDiscount.HasValue)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var couponUsage = CouponUsage.Create(
                couponDto.Id,
                userId,
                order.Id,
                new Money(order.CouponDiscount.Value));
            await _context.Set<CouponUsage>().AddAsync(couponUsage, cancellationToken);

            var couponEntity = await _context.Set<Coupon>()
                .FirstOrDefaultAsync(c => c.Id == couponDto.Id, cancellationToken);

            if (couponEntity != null)
            {
                couponEntity.IncrementUsage();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private decimal CalculateShippingCost(decimal subTotal)
    {
        return subTotal >= _orderSettings.FreeShippingThreshold
            ? 0
            : _orderSettings.DefaultShippingCost;
    }

    private decimal CalculateTax(decimal subTotal)
    {
        return subTotal * _orderSettings.TaxRate;
    }
}
