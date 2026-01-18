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
    public string Key { get; private set; } = string.Empty; // e.g., "button.add_to_cart", "header.welcome"
    public Guid LanguageId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty; // UI, Email, Notification, etc.

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Language Language { get; private set; } = null!;

    private StaticTranslation() { }

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
        // Configuration değerleri: MaxTranslationKeyLength=200, MaxTranslationValueLength=5000, MaxTranslationCategoryLength=50
        Guard.AgainstLength(key, 200, nameof(key));
        Guard.AgainstLength(value, 5000, nameof(value));
        Guard.AgainstLength(category, 50, nameof(category));

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

        translation.AddDomainEvent(new StaticTranslationCreatedEvent(translation.Id, key, languageCode, category));

        return translation;
    }

    public void UpdateKey(string newKey)
    {
        Guard.AgainstNullOrEmpty(newKey, nameof(newKey));
        // Configuration değeri: MaxTranslationKeyLength=200
        Guard.AgainstLength(newKey, 200, nameof(newKey));
        Key = newKey;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaticTranslationUpdatedEvent(Id, newKey, LanguageCode));
    }

    public void UpdateLanguage(Guid newLanguageId, string newLanguageCode)
    {
        Guard.AgainstDefault(newLanguageId, nameof(newLanguageId));
        Guard.AgainstNullOrEmpty(newLanguageCode, nameof(newLanguageCode));
        LanguageId = newLanguageId;
        LanguageCode = newLanguageCode.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaticTranslationUpdatedEvent(Id, Key, newLanguageCode));
    }

    public void UpdateCategory(string newCategory)
    {
        Guard.AgainstNullOrEmpty(newCategory, nameof(newCategory));
        // Configuration değeri: MaxTranslationCategoryLength=50
        Guard.AgainstLength(newCategory, 50, nameof(newCategory));
        Category = newCategory;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaticTranslationUpdatedEvent(Id, Key, LanguageCode));
    }

    public void UpdateValue(string value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));
        // Configuration değeri: MaxTranslationValueLength=5000
        Guard.AgainstLength(value, 5000, nameof(value));

        Value = value;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaticTranslationUpdatedEvent(Id, Key, LanguageCode));
    }

    public void Update(string value, string category = "UI")
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));
        Guard.AgainstNullOrEmpty(category, nameof(category));
        // Configuration değerleri: MaxTranslationValueLength=5000, MaxTranslationCategoryLength=50
        Guard.AgainstLength(value, 5000, nameof(value));
        Guard.AgainstLength(category, 50, nameof(category));

        Value = value;
        Category = category;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaticTranslationUpdatedEvent(Id, Key, LanguageCode));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new StaticTranslationDeletedEvent(Id, Key, LanguageCode));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new StaticTranslationRestoredEvent(Id, Key, LanguageCode));
    }
}

