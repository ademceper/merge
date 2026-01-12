using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Content;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record CMSPageDto(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string PageType,
    string Status,
    Guid? AuthorId,
    string? AuthorName,
    DateTime? PublishedAt,
    string? Template,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsHomePage,
    int DisplayOrder,
    bool ShowInMenu,
    string? MenuTitle,
    Guid? ParentPageId,
    string? ParentPageTitle,
    int ViewCount,
    DateTime CreatedAt
);
