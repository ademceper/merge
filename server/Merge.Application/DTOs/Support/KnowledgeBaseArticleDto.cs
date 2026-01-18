using System.Text.Json.Serialization;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// Knowledge Base Article DTO with HATEOAS links
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record KnowledgeBaseArticleDto(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    Guid? CategoryId,
    string? CategoryName,
    string Status,
    int ViewCount,
    int HelpfulCount,
    int NotHelpfulCount,
    bool IsFeatured,
    int DisplayOrder,
    IReadOnlyList<string> Tags,
    Guid? AuthorId,
    string? AuthorName,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    [property: JsonPropertyName("_links")]
    Dictionary<string, object>? Links = null
);
