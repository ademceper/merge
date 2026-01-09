using Merge.Domain.Common;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// UserLanguagePreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserLanguagePreference : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;

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

        return new UserLanguagePreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LanguageId = languageId,
            LanguageCode = languageCode.ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Update language preference
    public void UpdateLanguage(Guid languageId, string languageCode)
    {
        Guard.AgainstDefault(languageId, nameof(languageId));
        Guard.AgainstNullOrEmpty(languageCode, nameof(languageCode));

        LanguageId = languageId;
        LanguageCode = languageCode.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }
}

