namespace Merge.Domain.Entities;

/// <summary>
/// KnowledgeBaseView Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class KnowledgeBaseView : BaseEntity
{
    public Guid ArticleId { get; set; }
    public Guid? UserId { get; set; } // Nullable for anonymous views
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int ViewDuration { get; set; } = 0; // Seconds
    
    // Navigation properties
    public KnowledgeBaseArticle Article { get; set; } = null!;
    public User? User { get; set; }
}

