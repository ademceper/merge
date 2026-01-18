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
    public Guid UserId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Currency Currency { get; private set; } = null!;

    private UserCurrencyPreference() { }

    public static UserCurrencyPreference Create(
        Guid userId,
        Guid currencyId,
        string currencyCode)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(currencyId, nameof(currencyId));
        Guard.AgainstNullOrEmpty(currencyCode, nameof(currencyCode));
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
        
        preference.AddDomainEvent(new UserCurrencyPreferenceCreatedEvent(preference.Id, userId, currencyId, currencyCode));
        
        return preference;
    }

    public void UpdateCurrency(Guid currencyId, string currencyCode)
    {
        Guard.AgainstDefault(currencyId, nameof(currencyId));
        Guard.AgainstNullOrEmpty(currencyCode, nameof(currencyCode));
        // Configuration değeri: MaxUserCurrencyCodeLength=10
        Guard.AgainstLength(currencyCode, 10, nameof(currencyCode));

        CurrencyId = currencyId;
        CurrencyCode = currencyCode.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserCurrencyPreferenceUpdatedEvent(Id, UserId, currencyId, currencyCode));
    }
}

