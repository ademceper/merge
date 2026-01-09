namespace Merge.Application.DTOs.Content;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record SitemapEntryDto(
    Guid Id,
    string Url,
    string PageType,
    Guid? EntityId,
    DateTime LastModified,
    string ChangeFrequency,
    decimal Priority,
    bool IsActive
);
