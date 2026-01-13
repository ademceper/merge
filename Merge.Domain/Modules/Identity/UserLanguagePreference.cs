using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Content;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// UserLanguagePreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserLanguagePreference : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Language Language { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private UserLanguagePreference() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static UserLanguagePreference Create(
        Guid userId,
        Guid languageId,
        string languageCode)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(languageId, nameof(languageId));
        Guard.AgainstNullOrEmpty(languageCode, nameof(languageCode));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxUserLanguageCodeLength=10
        Guard.AgainstLength(languageCode, 10, nameof(languageCode));

        var preference = new UserLanguagePreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LanguageId = languageId,
            LanguageCode = languageCode.ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.5: Domain Events - UserLanguagePreferenceCreatedEvent
        preference.AddDomainEvent(new UserLanguagePreferenceCreatedEvent(preference.Id, userId, languageId, languageCode));
        
        return preference;
    }

    // ✅ BOLUM 1.1: Domain Method - Update language preference
    public void UpdateLanguage(Guid languageId, string languageCode)
    {
        Guard.AgainstDefault(languageId, nameof(languageId));
        Guard.AgainstNullOrEmpty(languageCode, nameof(languageCode));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxUserLanguageCodeLength=10
        Guard.AgainstLength(languageCode, 10, nameof(languageCode));

        LanguageId = languageId;
        LanguageCode = languageCode.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - UserLanguagePreferenceUpdatedEvent
        AddDomainEvent(new UserLanguagePreferenceUpdatedEvent(Id, UserId, languageId, languageCode));
    }
}

