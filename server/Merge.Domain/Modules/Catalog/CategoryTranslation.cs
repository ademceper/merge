using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Content;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// CategoryTranslation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CategoryTranslation : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CategoryId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Category Category { get; private set; } = null!;
    public Language Language { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CategoryTranslation() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static CategoryTranslation Create(
        Guid categoryId,
        Guid languageId,
        string languageCode,
        string name,
        string description = "")
    {
        Guard.AgainstDefault(categoryId, nameof(categoryId));
        Guard.AgainstDefault(languageId, nameof(languageId));
        Guard.AgainstNullOrEmpty(languageCode, nameof(languageCode));
        Guard.AgainstNullOrEmpty(name, nameof(name));

        var translation = new CategoryTranslation
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            LanguageId = languageId,
            LanguageCode = languageCode.ToLowerInvariant(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.4: Invariant validation
        translation.ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - CategoryTranslationCreatedEvent
        translation.AddDomainEvent(new CategoryTranslationCreatedEvent(translation.Id, categoryId, languageId, languageCode, name));
        
        return translation;
    }

    // ✅ BOLUM 1.1: Domain Method - Update translation
    public void Update(string name, string description = "")
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - CategoryTranslationUpdatedEvent
        AddDomainEvent(new CategoryTranslationUpdatedEvent(Id, CategoryId, LanguageCode));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - CategoryTranslationDeletedEvent
        AddDomainEvent(new CategoryTranslationDeletedEvent(Id, CategoryId, LanguageCode));
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (Guid.Empty == CategoryId)
            throw new DomainException("Kategori ID boş olamaz");

        if (Guid.Empty == LanguageId)
            throw new DomainException("Dil ID boş olamaz");

        if (string.IsNullOrWhiteSpace(LanguageCode))
            throw new DomainException("Dil kodu boş olamaz");

        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Çeviri adı boş olamaz");

        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxCategoryTranslationNameLength=200, MaxCategoryTranslationDescriptionLength=2000
        Guard.AgainstLength(Name, 200, nameof(Name));
        Guard.AgainstLength(Description, 2000, nameof(Description));
    }
}

