using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// Order aggregate root - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// </summary>
public class Order : BaseEntity, IAggregateRoot
{
    private readonly List<OrderItem> _orderItems = new();

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid AddressId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - EF Core compatibility için decimal backing fields
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
    
    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
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
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    
    public string PaymentMethod { get; private set; } = string.Empty;
    public DateTime? ShippedDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }
    public Guid? CouponId { get; private set; }
    public Guid? ParentOrderId { get; private set; }
    public bool IsSplitOrder { get; private set; } = false;

    // ✅ BOLUM 1.5: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // Navigation properties
    public User User { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Coupon? Coupon { get; private set; }
    public Payment? Payment { get; private set; }
    public Shipping? Shipping { get; private set; }
    public ICollection<ReturnRequest> ReturnRequests { get; private set; } = new List<ReturnRequest>();
    public Invoice? Invoice { get; private set; }
    public ICollection<GiftCardTransaction> GiftCardTransactions { get; private set; } = new List<GiftCardTransaction>();
    public Order? ParentOrder { get; private set; }
    public ICollection<Order> SplitOrders { get; private set; } = new List<Order>();
    public ICollection<OrderSplit> OriginalSplits { get; private set; } = new List<OrderSplit>();
    public ICollection<OrderSplit> SplitFrom { get; private set; } = new List<OrderSplit>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Order() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        // ✅ BOLUM 1.5: Domain Event - Order Created
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, userId, 0));

        return order;
    }

    // ✅ BOLUM 1.1: Domain Logic - Add item to order
    public void AddItem(Product product, int quantity)
    {
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        // ✅ BOLUM 1.1: Business rule - Can only add items to pending orders
        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişe ürün eklenemez");

        // ✅ BOLUM 1.4: Invariant validation - Stock check
        if (product.StockQuantity < quantity)
            throw new DomainException($"Yetersiz stok. Mevcut: {product.StockQuantity}, İstenen: {quantity}");

        var unitPrice = Money.Zero();
        if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
            unitPrice = new Money(product.DiscountPrice.Value);
        else
            unitPrice = new Money(product.Price);

        var totalPrice = new Money(unitPrice.Amount * quantity);

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = Id,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = unitPrice.Amount, // EF Core compatibility
            TotalPrice = totalPrice.Amount,
            CreatedAt = DateTime.UtcNow
        };

        _orderItems.Add(orderItem);
        RecalculateTotals();
    }

    // ✅ BOLUM 1.1: Domain Logic - Remove item from order
    public void RemoveItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişten ürün çıkarılamaz");

        var item = _orderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
            throw new DomainException("Sipariş öğesi bulunamadı");

        _orderItems.Remove(item);
        RecalculateTotals();
    }

    // ✅ BOLUM 1.1: Domain Logic - Update item quantity
    public void UpdateItemQuantity(Guid orderItemId, int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişte ürün miktarı değiştirilemez");

        var item = _orderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
            throw new DomainException("Sipariş öğesi bulunamadı");

        // Stock check would require product lookup - handled in service layer
        item.Quantity = newQuantity;
        item.TotalPrice = item.UnitPrice * newQuantity;
        RecalculateTotals();
    }

    // ✅ BOLUM 1.1: Domain Logic - Apply coupon
    public void ApplyCoupon(Coupon coupon, Money discountAmount)
    {
        Guard.AgainstNull(coupon, nameof(coupon));

        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişe kupon uygulanamaz");

        if (!coupon.IsActive)
            throw new DomainException("Kupon aktif değil");

        if (coupon.UsedCount >= coupon.UsageLimit && coupon.UsageLimit > 0)
            throw new DomainException("Kupon kullanım limitine ulaşıldı");

        if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
            throw new DomainException("Kupon geçerli tarih aralığında değil");

        // ✅ BOLUM 1.4: Invariant validation - Minimum purchase amount
        if (coupon.MinimumPurchaseAmount.HasValue && _subTotal < coupon.MinimumPurchaseAmount.Value)
            throw new DomainException($"Minimum alışveriş tutarı: {coupon.MinimumPurchaseAmount.Value} TL");

        CouponId = coupon.Id;
        _couponDiscount = discountAmount.Amount;
        RecalculateTotals();
    }

    // ✅ BOLUM 1.1: Domain Logic - Remove coupon
    public void RemoveCoupon()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişten kupon kaldırılamaz");

        CouponId = null;
        _couponDiscount = null;
        RecalculateTotals();
    }

    // ✅ BOLUM 1.1: Domain Logic - Apply gift card discount
    public void ApplyGiftCardDiscount(Money discountAmount)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Bekleyen olmayan siparişe hediye kartı uygulanamaz");

        if (discountAmount.Amount > _totalAmount)
            throw new DomainException("Hediye kartı tutarı sipariş tutarından fazla olamaz");

        _giftCardDiscount = discountAmount.Amount;
        RecalculateTotals();
    }

    // ✅ BOLUM 1.1: State Machine Pattern - Transition to new status
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

    // ✅ BOLUM 1.1: Domain Logic - Convenience methods for common transitions
    public void Confirm()
    {
        TransitionTo(OrderStatus.Processing);
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new DomainException("Sipariş işleme durumunda olmalıdır");

        TransitionTo(OrderStatus.Shipped);
        
        // ✅ BOLUM 1.5: Domain Event - Order Shipped
        AddDomainEvent(new OrderShippedEvent(Id, UserId, ShippedDate ?? DateTime.UtcNow));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Sipariş kargoya verilmiş durumunda olmalıdır");

        TransitionTo(OrderStatus.Delivered);
        
        // ✅ BOLUM 1.5: Domain Event - Order Delivered
        AddDomainEvent(new OrderDeliveredEvent(Id, UserId, DeliveredDate ?? DateTime.UtcNow));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new DomainException("Kargoya verilmiş veya teslim edilmiş sipariş iptal edilemez");

        TransitionTo(OrderStatus.Cancelled);
        
        // ✅ BOLUM 1.5: Domain Event - Order Cancelled
        AddDomainEvent(new OrderCancelledEvent(Id, UserId, reason));
    }

    public void Refund()
    {
        if (Status != OrderStatus.Delivered)
            throw new DomainException("Sadece teslim edilmiş siparişler iade edilebilir");

        TransitionTo(OrderStatus.Refunded);
    }

    public void PutOnHold()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Sadece bekleyen siparişler beklemeye alınabilir");

        TransitionTo(OrderStatus.OnHold);
    }

    // ✅ BOLUM 1.1: Domain Logic - Set payment status
    public void SetPaymentStatus(PaymentStatus status)
    {
        PaymentStatus = status;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set payment method
    public void SetPaymentMethod(string paymentMethod)
    {
        Guard.AgainstNullOrEmpty(paymentMethod, nameof(paymentMethod));
        PaymentMethod = paymentMethod;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Calculate totals
    // ✅ BOLUM 1.1: Domain Logic - Recalculate totals (public for service layer usage)
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
        
        // ✅ BOLUM 1.4: Invariant validation - Total must be non-negative
        if (total < 0)
            throw new DomainException("Sipariş tutarı negatif olamaz");
        
        _totalAmount = total;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set shipping cost
    public void SetShippingCost(Money shippingCost)
    {
        Guard.AgainstNegative(shippingCost.Amount, nameof(shippingCost));
        _shippingCost = shippingCost.Amount;
        RecalculateTotals();
    }

    // ✅ BOLUM 1.1: Domain Logic - Set tax
    public void SetTax(Money tax)
    {
        Guard.AgainstNegative(tax.Amount, nameof(tax));
        _tax = tax.Amount;
        RecalculateTotals();
    }

    // ✅ BOLUM 1.4: Invariant validation - Total amount must be non-negative
    private void ValidateInvariants()
    {
        if (_totalAmount < 0)
            throw new DomainException("Sipariş tutarı negatif olamaz");

        if (_orderItems.Count == 0 && Status != OrderStatus.Cancelled)
            throw new DomainException("Sipariş en az bir ürün içermelidir");
    }

    // ✅ BOLUM 1.1: Helper method - Generate order number
    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}

