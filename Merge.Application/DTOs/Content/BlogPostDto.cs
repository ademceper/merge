using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Content;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record BlogPostDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string Slug,
    string Excerpt,
    string Content,
    string? FeaturedImageUrl,
    string Status,
    DateTime? PublishedAt,
    int ViewCount,
    int LikeCount,
    int CommentCount,
    IReadOnlyList<string> Tags,
    bool IsFeatured,
    bool AllowComments,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? OgImageUrl,
    int ReadingTimeMinutes,
    DateTime CreatedAt
);
