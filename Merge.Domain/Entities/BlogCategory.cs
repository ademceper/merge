namespace Merge.Domain.Entities;

/// <summary>
/// BlogCategory Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class BlogCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public BlogCategory? ParentCategory { get; set; }
    public ICollection<BlogCategory> SubCategories { get; set; } = new List<BlogCategory>();
    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

