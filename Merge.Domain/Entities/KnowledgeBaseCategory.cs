namespace Merge.Domain.Entities;

/// <summary>
/// KnowledgeBaseCategory Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class KnowledgeBaseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? IconUrl { get; set; }
    
    // Navigation properties
    public KnowledgeBaseCategory? ParentCategory { get; set; }
    public ICollection<KnowledgeBaseCategory> SubCategories { get; set; } = new List<KnowledgeBaseCategory>();
    public ICollection<KnowledgeBaseArticle> Articles { get; set; } = new List<KnowledgeBaseArticle>();
}

