namespace Merge.Application.DTOs.Content;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record BlogCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    IReadOnlyList<BlogCategoryDto>? SubCategories,
    int PostCount,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt
);
