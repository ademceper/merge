using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Ordering;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// GiftCardTransaction Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class GiftCardTransaction : BaseEntity
{
    public Guid GiftCardId { get; private set; }
    public Guid? OrderId { get; private set; }
    
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
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public GiftCard GiftCard { get; private set; } = null!;
    public Order? Order { get; private set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money AmountMoney => new Money(_amount);

    private GiftCardTransaction() { }

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

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

