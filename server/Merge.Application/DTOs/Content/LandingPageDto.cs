using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Content;

public record LandingPageDto(
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
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive,
    string? MetaTitle,
    string? MetaDescription,
    string? OgImageUrl,
    int ViewCount,
    int ConversionCount,
    decimal ConversionRate,
    bool EnableABTesting,
    Guid? VariantOfId,
    int TrafficSplit,
    DateTime CreatedAt
);
