using System.Text.Json.Serialization;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// Knowledge Base Category DTO with HATEOAS links
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record KnowledgeBaseCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    int DisplayOrder,
    bool IsActive,
    string? IconUrl,
    int ArticleCount,
    IReadOnlyList<KnowledgeBaseCategoryDto> SubCategories,
    DateTime CreatedAt,
    // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
    [property: JsonPropertyName("_links")]
    Dictionary<string, object>? Links = null
);
