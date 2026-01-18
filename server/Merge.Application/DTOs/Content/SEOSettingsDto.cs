namespace Merge.Application.DTOs.Content;

// StructuredData JSON string olarak döndürülür (güvenlik için)
public record SEOSettingsDto(
    Guid Id,
    string PageType,
    Guid? EntityId,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? CanonicalUrl,
    string? OgTitle,
    string? OgDescription,
    string? OgImageUrl,
    string? TwitterCard,
    string? StructuredDataJson, // JSON string (Dictionary yerine)
    bool IsIndexed,
    bool FollowLinks,
    decimal Priority,
    string? ChangeFrequency,
    DateTime CreatedAt
);
