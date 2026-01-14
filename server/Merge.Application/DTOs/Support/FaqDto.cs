using System.Text.Json.Serialization;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// FAQ DTO with HATEOAS links
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record FaqDto(
    Guid Id,
    string Question,
    string Answer,
    string Category,
    int SortOrder,
    int ViewCount,
    bool IsPublished,
    // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
    [property: JsonPropertyName("_links")]
    Dictionary<string, object>? Links = null
);
