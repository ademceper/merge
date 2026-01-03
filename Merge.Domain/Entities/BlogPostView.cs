namespace Merge.Domain.Entities;

/// <summary>
/// BlogPostView Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class BlogPostView : BaseEntity
{
    public Guid BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;
    public Guid? UserId { get; set; } // Nullable for anonymous views
    public User? User { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int ViewDurationSeconds { get; set; } = 0; // How long user viewed the post
}

