using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// CouponUsage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CouponUsage : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CouponId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrderId { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects - Money backing field (EF Core compatibility)
    private decimal _discountAmount;
    public decimal DiscountAmount 
    { 
        get => _discountAmount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(DiscountAmount));
            _discountAmount = value;
        } 
    }
    
    // Navigation properties
    public Coupon Coupon { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Order Order { get; private set; } = null!;

    // ✅ BOLUM 1.3: Value Object properties
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money DiscountAmountMoney => new Money(_discountAmount);

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CouponUsage() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static CouponUsage Create(
        Guid couponId,
        Guid userId,
        Guid orderId,
        Money discountAmount)
    {
        Guard.AgainstDefault(couponId, nameof(couponId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNull(discountAmount, nameof(discountAmount));

        return new CouponUsage
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            UserId = userId,
            OrderId = orderId,
            _discountAmount = discountAmount.Amount,
            CreatedAt = DateTime.UtcNow
        };
    }
}

