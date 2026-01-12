using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// StaticTranslation Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class StaticTranslation : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Key { get; private set; } = string.Empty; // e.g., "button.add_to_cart", "header.welcome"
    public Guid LanguageId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty; // UI, Email, Notification, etc.

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Language Language { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private StaticTranslation() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static StaticTranslation Create(
        string key,
        Guid languageId,
        string languageCode,
        string value,
        string category = "UI")
    {
        Guard.AgainstNullOrEmpty(key, nameof(key));
        Guard.AgainstDefault(languageId, nameof(languageId));
        Guard.AgainstNullOrEmpty(languageCode, nameof(languageCode));
        Guard.AgainstNullOrEmpty(value, nameof(value));
        Guard.AgainstNullOrEmpty(category, nameof(category));

        var translation = new StaticTranslation
        {
            Id = Guid.NewGuid(),
            Key = key,
            LanguageId = languageId,
            LanguageCode = languageCode.ToLowerInvariant(),
            Value = value,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - StaticTranslationCreatedEvent yayınla (ÖNERİLİR)
        translation.AddDomainEvent(new StaticTranslationCreatedEvent(translation.Id, key, languageCode, category));

        return translation;
    }

    // ✅ BOLUM 1.1: Domain Method - Update translation value
    public void UpdateValue(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));

        Value = value;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - StaticTranslationUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new StaticTranslationUpdatedEvent(Id, Key, LanguageCode));
    }

    // ✅ BOLUM 1.1: Domain Method - Update translation
    public void Update(string value, string category = "UI")
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));
        Guard.AgainstNullOrEmpty(category, nameof(category));

        Value = value;
        Category = category;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - StaticTranslationUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new StaticTranslationUpdatedEvent(Id, Key, LanguageCode));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - StaticTranslationDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new StaticTranslationDeletedEvent(Id, Key, LanguageCode));
    }
}

