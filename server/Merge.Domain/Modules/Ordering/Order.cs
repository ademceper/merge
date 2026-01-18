using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Payment;
using Address = Merge.Domain.Modules.Identity.Address;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// Order aggregate root - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Order : BaseEntity, IAggregateRoot
{
    private readonly List<OrderItem> _orderItems = new();

    public Guid UserId { get; private set; }
    public Guid AddressId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    
    private decimal _subTotal;
    private decimal _shippingCost;
    private decimal _tax;
    private decimal _totalAmount;
    private decimal? _couponDiscount;
    private decimal? _giftCardDiscount;
    
    // Database columns (EF Core mapping)
    public decimal SubTotal 
    { 
        get => _subTotal; 
        private set => _subTotal = value; 
    }
    
    public decimal ShippingCost 
    { 
        get => _shippingCost; 
        private set => _shippingCost = value; 
    }
    
    public decimal Tax 
    { 
        get => _tax; 
        private set => _tax = value; 
    }
    
    public decimal TotalAmount 
    { 
        get => _totalAmount; 
        private set => _totalAmount = value; 
    }
    
    public decimal? CouponDiscount 
    { 
        get => _couponDiscount; 
        private set => _couponDiscount = value; 
    }
    
    public decimal? GiftCardDiscount 
    { 
        get => _giftCardDiscount; 
        private set => _giftCardDiscount = value; 
    }
    
    [NotMapped]
    public Money SubTotalMoney => new Money(_subTotal);
    
    [NotMapped]
    public Money ShippingCostMoney => new Money(_shippingCost);
    
    [NotMapped]
    public Money TaxMoney => new Money(_tax);
    
    [NotMapped]
    public Money TotalAmountMoney => new Money(_totalAmount);
    
    [NotMapped]
    public Money? CouponDiscountMoney => _couponDiscount.HasValue ? new Money(_couponDiscount.Value) : null;
    
    [NotMapped]
    public Money? GiftCardDiscountMoney => _giftCardDiscount.HasValue ? new Money(_giftCardDiscount.Value) : null;
    
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    
    public string PaymentMethod { get; private set; } = string.Empty;
    public DateTime? ShippedDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }
    public Guid? CouponId { get; private set; }
    public Guid? ParentOrderId { get; private set; }
    public bool IsSplitOrder { get; private set; } = false;

    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı (Team entity'sinde de aynı pattern kullanılıyor)
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // Navigation properties
    public User User { get; private set; } = null!;
    public Address Address { get; private set; } = null!; 
    public Coupon? Coupon { get; private set; }
    public PaymentEntity? Payment { get; private set; }
    public Shipping? Shipping { get; private set; }
    public ICollection<ReturnRequest> ReturnRequests { get; private set; } = [];
    public Invoice? Invoice { get; private set; }
    public ICollection<GiftCardTransaction> GiftCardTransactions { get; private set; } = [];
    public Order? ParentOrder { get; private set; }
    public ICollection<Order> SplitOrders { get; private set; } = [];
    public ICollection<OrderSplit> OriginalSplits { get; private set; } = [];
    public ICollection<OrderSplit> SplitFrom { get; private set; } = [];

    private Order() { }

    public static Order Create(Guid userId, Guid addressId, Address address)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(addressId, nameof(addressId));
        Guard.AgainstNull(address, nameof(address));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AddressId = addressId,
            Address = address,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Not: TotalAmount henüz hesaplanmadı (items eklenmedi), 0 olarak gönderiliyor
        // Event handler'da order reload edilerek gerçek TotalAmount alınabilir
        // Alternatif: Items eklendikten sonra event dispatch edilebilir ama bu domain event pattern'e aykırı
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, userId, 0));

        return order;
    }

    public void AddItem(Product product, int quantity)
    {
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişe ürün eklenemez");

        if (product.StockQuantity < quantity)
            throw new DomainException($"Yetersiz stok. Mevcut: {product.StockQuantity}, İstenen: {quantity}");

        var unitPrice = Money.Zero();
        if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
            unitPrice = new Money(product.DiscountPrice.Value);
        else
            unitPrice = new Money(product.Price);

        var orderItem = OrderItem.Create(
            Id,
            product.Id,
            product,
            quantity,
            unitPrice);

        _orderItems.Add(orderItem);
        RecalculateTotals();
        
        ValidateInvariants();
        
        AddDomainEvent(new OrderItemAddedEvent(
            Id,
            UserId,
            orderItem.Id,
            product.Id,
            quantity,
            unitPrice.Amount,
            orderItem.TotalPrice));
    }

    public void RemoveItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişten ürün çıkarılamaz");

        var item = _orderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item is null)
            throw new DomainException("Sipariş öğesi bulunamadı");

        var productId = item.ProductId;
        _orderItems.Remove(item);
        RecalculateTotals();
        
        ValidateInvariants();
        
        AddDomainEvent(new OrderItemRemovedEvent(Id, UserId, orderItemId, productId));
    }

    public void UpdateItemQuantity(Guid orderItemId, int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişte ürün miktarı değiştirilemez");

        var item = _orderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item is null)
            throw new DomainException("Sipariş öğesi bulunamadı");

        // Stock check would require product lookup - handled in service layer
        var oldQuantity = item.Quantity;
        var oldTotalPrice = item.TotalPrice;
        item.UpdateQuantity(newQuantity);
        RecalculateTotals();
        
        ValidateInvariants();
        
        AddDomainEvent(new OrderItemUpdatedEvent(
            Id,
            UserId,
            orderItemId,
            item.ProductId,
            oldQuantity,
            newQuantity,
            oldTotalPrice,
            item.TotalPrice));
    }

    public void ApplyCoupon(Coupon coupon, Money discountAmount)
    {
        Guard.AgainstNull(coupon, nameof(coupon));
        Guard.AgainstNull(discountAmount, nameof(discountAmount));

        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişe kupon uygulanamaz");

        if (!coupon.IsActive)
            throw new DomainException("Kupon aktif değil");

        if (coupon.UsedCount >= coupon.UsageLimit && coupon.UsageLimit > 0)
            throw new DomainException("Kupon kullanım limitine ulaşıldı");

        if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
            throw new DomainException("Kupon geçerli tarih aralığında değil");

        if (coupon.MinimumPurchaseAmount.HasValue && _subTotal < coupon.MinimumPurchaseAmount.Value)
            throw new DomainException($"Minimum alışveriş tutarı: {coupon.MinimumPurchaseAmount.Value} TL");

        CouponId = coupon.Id;
        _couponDiscount = discountAmount.Amount;
        RecalculateTotals();
        
        AddDomainEvent(new OrderCouponAppliedEvent(Id, UserId, coupon.Id, discountAmount.Amount));
    }

    public void RemoveCoupon()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişten kupon kaldırılamaz");

        var removedCouponId = CouponId;
        CouponId = null;
        _couponDiscount = null;
        RecalculateTotals();
        
        AddDomainEvent(new OrderCouponRemovedEvent(Id, UserId, removedCouponId));
    }

    public void ApplyGiftCardDiscount(Money discountAmount)
    {
        Guard.AgainstNull(discountAmount, nameof(discountAmount));
        
        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişe hediye kartı uygulanamaz");

        // Not: _totalAmount kontrolü RecalculateTotals() sonrası yapılmalı
        // Önce mevcut total'ı kontrol et, sonra discount'u uygula
        var currentTotal = _subTotal - (_couponDiscount ?? 0) + _shippingCost + _tax;
        if (discountAmount.Amount > currentTotal)
            throw new DomainException("Hediye kartı tutarı sipariş tutarından fazla olamaz");

        _giftCardDiscount = discountAmount.Amount;
        RecalculateTotals();
        
        ValidateInvariants();
        
        AddDomainEvent(new OrderGiftCardDiscountAppliedEvent(Id, UserId, discountAmount.Amount));
    }

    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        { OrderStatus.Pending, new[] { OrderStatus.Processing, OrderStatus.Cancelled, OrderStatus.OnHold } },
        { OrderStatus.Processing, new[] { OrderStatus.Shipped, OrderStatus.Cancelled } },
        { OrderStatus.Shipped, new[] { OrderStatus.Delivered } },
        { OrderStatus.Delivered, new[] { OrderStatus.Refunded } },
        { OrderStatus.OnHold, new[] { OrderStatus.Pending, OrderStatus.Cancelled } },
        { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }, // Terminal state
        { OrderStatus.Refunded, Array.Empty<OrderStatus>() } // Terminal state
    };

    public void TransitionTo(OrderStatus newStatus)
    {
        if (!AllowedTransitions.ContainsKey(Status))
            throw new InvalidStateTransitionException(Status, newStatus);

        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new InvalidStateTransitionException(Status, newStatus);

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        // Set dates based on status
        if (newStatus == OrderStatus.Shipped)
            ShippedDate = DateTime.UtcNow;
        else if (newStatus == OrderStatus.Delivered)
            DeliveredDate = DateTime.UtcNow;
    }

    public void Confirm()
    {
        TransitionTo(OrderStatus.Processing);
        
        AddDomainEvent(new OrderConfirmedEvent(Id, UserId));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new DomainException("Sipariş işleme durumunda olmalıdır");

        TransitionTo(OrderStatus.Shipped);
        
        AddDomainEvent(new OrderShippedEvent(Id, UserId, ShippedDate ?? DateTime.UtcNow));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Sipariş kargoya verilmiş durumunda olmalıdır");

        TransitionTo(OrderStatus.Delivered);
        
        AddDomainEvent(new OrderDeliveredEvent(Id, UserId, DeliveredDate ?? DateTime.UtcNow));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new DomainException("Kargoya verilmiş veya teslim edilmiş sipariş iptal edilemez");

        TransitionTo(OrderStatus.Cancelled);
        
        AddDomainEvent(new OrderCancelledEvent(Id, UserId, reason));
    }

    public void Refund()
    {
        if (Status != OrderStatus.Delivered)
            throw new DomainException("Sadece teslim edilmiş siparişler iade edilebilir");

        TransitionTo(OrderStatus.Refunded);
        
        AddDomainEvent(new OrderRefundedEvent(Id, UserId, TotalAmount));
    }

    public void PutOnHold(string? reason = null)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Sadece bekleyen siparişler beklemeye alınabilir");

        TransitionTo(OrderStatus.OnHold);
        
        AddDomainEvent(new OrderPutOnHoldEvent(Id, UserId, reason));
    }

    public void SetPaymentStatus(PaymentStatus status)
    {
        var oldStatus = PaymentStatus;
        PaymentStatus = status;
        UpdatedAt = DateTime.UtcNow;
        
        if (oldStatus != status)
            AddDomainEvent(new OrderPaymentStatusChangedEvent(Id, UserId, oldStatus, status));
    }

    public void SetPaymentMethod(string paymentMethod)
    {
        Guard.AgainstNullOrEmpty(paymentMethod, nameof(paymentMethod));
        
        var oldMethod = PaymentMethod;
        PaymentMethod = paymentMethod;
        UpdatedAt = DateTime.UtcNow;
        
        if (oldMethod != paymentMethod)
            AddDomainEvent(new OrderPaymentMethodChangedEvent(Id, UserId, oldMethod, paymentMethod));
    }

    public void RecalculateTotals()
    {
        // Calculate subtotal from items
        _subTotal = _orderItems.Sum(i => i.TotalPrice);

        // Calculate total with discounts
        var total = _subTotal;
        
        if (_couponDiscount.HasValue)
            total -= _couponDiscount.Value;
        
        if (_giftCardDiscount.HasValue)
            total -= _giftCardDiscount.Value;

        // Add shipping and tax
        total += _shippingCost + _tax;
        
        if (total < 0)
            throw new DomainException("Sipariş tutarı negatif olamaz");
        
        _totalAmount = total;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void SetShippingCost(Money shippingCost)
    {
        Guard.AgainstNull(shippingCost, nameof(shippingCost));
        Guard.AgainstNegative(shippingCost.Amount, nameof(shippingCost));
        var oldShippingCost = _shippingCost;
        _shippingCost = shippingCost.Amount;
        RecalculateTotals();
        
        if (oldShippingCost != _shippingCost)
            AddDomainEvent(new OrderTotalsRecalculatedEvent(
                Id,
                UserId,
                _subTotal,
                _shippingCost,
                _tax,
                _couponDiscount,
                _giftCardDiscount,
                _totalAmount));
    }

    public void SetTax(Money tax)
    {
        Guard.AgainstNull(tax, nameof(tax));
        Guard.AgainstNegative(tax.Amount, nameof(tax));
        var oldTax = _tax;
        _tax = tax.Amount;
        RecalculateTotals();
        
        if (oldTax != _tax)
            AddDomainEvent(new OrderTotalsRecalculatedEvent(
                Id,
                UserId,
                _subTotal,
                _shippingCost,
                _tax,
                _couponDiscount,
                _giftCardDiscount,
                _totalAmount));
    }

    private void ValidateInvariants()
    {
        if (_totalAmount < 0)
            throw new DomainException("Sipariş tutarı negatif olamaz");

        if (_orderItems.Count == 0 && Status != OrderStatus.Cancelled)
            throw new DomainException("Sipariş en az bir ürün içermelidir");
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}

