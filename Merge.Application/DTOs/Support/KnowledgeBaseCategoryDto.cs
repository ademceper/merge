namespace Merge.Application.DTOs.Support;

public class KnowledgeBaseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? IconUrl { get; set; }
    public int ArticleCount { get; set; }
    public List<KnowledgeBaseCategoryDto> SubCategories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
