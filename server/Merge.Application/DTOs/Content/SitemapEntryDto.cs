namespace Merge.Application.DTOs.Content;

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
