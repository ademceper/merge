using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// GiftCardTransaction Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class GiftCardTransaction : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid GiftCardId { get; private set; }
    public Guid? OrderId { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects - Money backing field (EF Core compatibility)
    private decimal _amount;
    public decimal Amount 
    { 
        get => _amount; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Amount));
            _amount = value;
        } 
    }
    
    public GiftCardTransactionType Type { get; private set; }
    public string? Notes { get; private set; }
    
    // Navigation properties
    public GiftCard GiftCard { get; private set; } = null!;
    public Order? Order { get; private set; }

    // ✅ BOLUM 1.3: Value Object properties
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money AmountMoney => new Money(_amount);

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private GiftCardTransaction() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static GiftCardTransaction Create(
        Guid giftCardId,
        Money amount,
        GiftCardTransactionType type,
        Guid? orderId = null,
        string? notes = null)
    {
        Guard.AgainstDefault(giftCardId, nameof(giftCardId));
        Guard.AgainstNull(amount, nameof(amount));

        return new GiftCardTransaction
        {
            Id = Guid.NewGuid(),
            GiftCardId = giftCardId,
            _amount = amount.Amount,
            Type = type,
            OrderId = orderId,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Update notes
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

