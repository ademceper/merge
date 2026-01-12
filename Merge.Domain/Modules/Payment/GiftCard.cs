using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// GiftCard aggregate root - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class GiftCard : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Code { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Money
    private decimal _amount;
    private decimal _remainingAmount;
    
    public decimal Amount 
    { 
        get => _amount; 
        private set 
        { 
            Guard.AgainstNegativeOrZero(value, nameof(Amount));
            _amount = value;
        } 
    }
    
    // ✅ BOLUM 1.4: Invariant validation - RemainingAmount >= 0
    public decimal RemainingAmount 
    { 
        get => _remainingAmount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(RemainingAmount));
            _remainingAmount = value;
        } 
    }
    
    public Guid? PurchasedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? Message { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsRedeemed { get; private set; } = false;
    public DateTime? RedeemedAt { get; private set; }

    // ✅ BOLUM 1.5: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.3: Value Object properties
    [NotMapped]
    public Money AmountMoney => new Money(_amount);
    
    [NotMapped]
    public Money RemainingAmountMoney => new Money(_remainingAmount);

    // Navigation properties
    public User? PurchasedBy { get; private set; }
    public User? AssignedTo { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private GiftCard() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static GiftCard Create(
        string code,
        Money amount,
        DateTime expiresAt,
        Guid? purchasedByUserId = null,
        Guid? assignedToUserId = null,
        string? message = null)
    {
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstNull(amount, nameof(amount));

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Son kullanma tarihi gelecekte olmalıdır");

        var giftCard = new GiftCard
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant(),
            _amount = amount.Amount,
            _remainingAmount = amount.Amount,
            ExpiresAt = expiresAt,
            PurchasedByUserId = purchasedByUserId,
            AssignedToUserId = assignedToUserId,
            Message = message,
            IsActive = true,
            IsRedeemed = false,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - GiftCardCreatedEvent
        giftCard.AddDomainEvent(new GiftCardCreatedEvent(giftCard.Id, giftCard.Code, giftCard.Amount, giftCard.PurchasedByUserId, giftCard.AssignedToUserId));

        return giftCard;
    }

    // ✅ BOLUM 1.1: Domain Logic - Use gift card
    public void Use(Money amount)
    {
        Guard.AgainstNull(amount, nameof(amount));

        if (!IsActive)
            throw new DomainException("Hediye kartı aktif değil");

        if (IsRedeemed)
            throw new DomainException("Hediye kartı zaten kullanılmış");

        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("Hediye kartı süresi dolmuş");

        // ✅ BOLUM 1.4: Invariant validation - RemainingAmount >= 0
        if (amount.Amount > _remainingAmount)
            throw new DomainException($"Yetersiz hediye kartı bakiyesi. Mevcut: {_remainingAmount} TL, İstenen: {amount.Amount} TL");

        _remainingAmount -= amount.Amount;

        if (_remainingAmount == 0)
        {
            IsRedeemed = true;
            RedeemedAt = DateTime.UtcNow;
        }

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - GiftCardUsedEvent
        AddDomainEvent(new GiftCardUsedEvent(Id, Code, amount.Amount, _remainingAmount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Redeem gift card (full amount)
    public void Redeem()
    {
        if (!IsActive)
            throw new DomainException("Hediye kartı aktif değil");

        if (IsRedeemed)
            throw new DomainException("Hediye kartı zaten kullanılmış");

        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("Hediye kartı süresi dolmuş");

        IsRedeemed = true;
        _remainingAmount = 0;
        RedeemedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - GiftCardRedeemedEvent
        AddDomainEvent(new GiftCardRedeemedEvent(Id, Code, AssignedToUserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if gift card is valid
    public bool IsValid()
    {
        return IsActive && !IsRedeemed && DateTime.UtcNow <= ExpiresAt && _remainingAmount > 0;
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if gift card has sufficient balance
    public bool HasSufficientBalance(Money amount)
    {
        return _remainingAmount >= amount.Amount;
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate gift card
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - GiftCardActivatedEvent
        AddDomainEvent(new GiftCardActivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate gift card
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - GiftCardDeactivatedEvent
        AddDomainEvent(new GiftCardDeactivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Logic - Assign to user
    public void AssignTo(Guid userId)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }
}

