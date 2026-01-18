using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// Language Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Language : BaseEntity, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty; // en, tr, ar, de, fr
    public string Name { get; private set; } = string.Empty; // English, Türkçe, العربية
    public string NativeName { get; private set; } = string.Empty; // English, Türkçe, العربية
    public bool IsDefault { get; private set; } = false;
    public bool IsActive { get; private set; } = true;
    public bool IsRTL { get; private set; } = false; // Right-to-left (Arabic, Hebrew)
    public string FlagIcon { get; private set; } = string.Empty; // URL or emoji flag

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private Language() { }

    public static Language Create(
        string code,
        string name,
        string nativeName,
        bool isDefault = false,
        bool isActive = true,
        bool isRTL = false,
        string flagIcon = "")
    {
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(nativeName, nameof(nativeName));
        // Configuration değerleri: MinLanguageCodeLength=2, MaxLanguageCodeLength=10, MaxLanguageNameLength=100
        Guard.AgainstOutOfRange(code.Length, 2, 10, nameof(code));
        Guard.AgainstLength(name, 100, nameof(name));
        Guard.AgainstLength(nativeName, 100, nameof(nativeName));

        var language = new Language
        {
            Id = Guid.NewGuid(),
            Code = code.ToLowerInvariant(),
            Name = name,
            NativeName = nativeName,
            IsDefault = isDefault,
            IsActive = isActive,
            IsRTL = isRTL,
            FlagIcon = flagIcon,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        language.AddDomainEvent(new LanguageCreatedEvent(language.Id, language.Code, language.Name));

        return language;
    }

    public void UpdateDetails(string name, string nativeName, bool isRTL, string flagIcon)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(nativeName, nameof(nativeName));
        // Configuration değeri: MaxLanguageNameLength=100
        Guard.AgainstLength(name, 100, nameof(name));
        Guard.AgainstLength(nativeName, 100, nameof(nativeName));

        // Eğer URL formatındaysa validasyon yapılır, emoji ise geçer
        if (!string.IsNullOrEmpty(flagIcon) && flagIcon.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !IsValidUrl(flagIcon))
        {
            throw new DomainException("Geçerli bir flag icon URL giriniz.");
        }

        Name = name;
        NativeName = nativeName;
        IsRTL = isRTL;
        FlagIcon = flagIcon;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LanguageUpdatedEvent(Id, Code, Name));
    }

    public void SetAsDefault()
    {
        if (IsDefault)
            return;

        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LanguageSetAsDefaultEvent(Id, Code));
    }

    public void RemoveDefaultStatus()
    {
        if (!IsDefault)
            return;

        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new LanguageUpdatedEvent(Id, Code, Name));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LanguageActivatedEvent(Id, Code));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        if (IsDefault)
            throw new DomainException("Default language cannot be deactivated");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LanguageDeactivatedEvent(Id, Code));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        if (IsDefault)
            throw new DomainException("Default language cannot be deleted");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LanguageDeletedEvent(Id, Code));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new LanguageRestoredEvent(Id, Code));
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

