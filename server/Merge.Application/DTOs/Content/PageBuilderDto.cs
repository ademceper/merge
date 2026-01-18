using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Content;

public record PageBuilderDto(
    Guid Id,
    string Name,
    string Slug,
    string Title,
    string Content,
    string? Template,
    string Status,
    Guid? AuthorId,
    string? AuthorName,
    DateTime? PublishedAt,
    bool IsActive,
    string? MetaTitle,
    string? MetaDescription,
    string? OgImageUrl,
    int ViewCount,
    string? PageType,
    Guid? RelatedEntityId,
    DateTime CreatedAt
);
