using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Payment;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// UserCurrencyPreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserCurrencyPreference : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Currency Currency { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private UserCurrencyPreference() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static UserCurrencyPreference Create(
        Guid userId,
        Guid currencyId,
        string currencyCode)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(currencyId, nameof(currencyId));
        Guard.AgainstNullOrEmpty(currencyCode, nameof(currencyCode));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxUserCurrencyCodeLength=10
        Guard.AgainstLength(currencyCode, 10, nameof(currencyCode));

        var preference = new UserCurrencyPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CurrencyId = currencyId,
            CurrencyCode = currencyCode.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.5: Domain Events - UserCurrencyPreferenceCreatedEvent
        preference.AddDomainEvent(new UserCurrencyPreferenceCreatedEvent(preference.Id, userId, currencyId, currencyCode));
        
        return preference;
    }

    // ✅ BOLUM 1.1: Domain Method - Update currency preference
    public void UpdateCurrency(Guid currencyId, string currencyCode)
    {
        Guard.AgainstDefault(currencyId, nameof(currencyId));
        Guard.AgainstNullOrEmpty(currencyCode, nameof(currencyCode));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxUserCurrencyCodeLength=10
        Guard.AgainstLength(currencyCode, 10, nameof(currencyCode));

        CurrencyId = currencyId;
        CurrencyCode = currencyCode.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - UserCurrencyPreferenceUpdatedEvent
        AddDomainEvent(new UserCurrencyPreferenceUpdatedEvent(Id, UserId, currencyId, currencyCode));
    }
}

