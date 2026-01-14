namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record KnowledgeBaseSearchDto(
    string Query,
    Guid? CategoryId,
    bool FeaturedOnly = false,
    int Page = 1,
    int PageSize = 20
);
