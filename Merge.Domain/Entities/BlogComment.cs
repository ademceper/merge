namespace Merge.Domain.Entities;

/// <summary>
/// BlogComment Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class BlogComment : BaseEntity
{
    public Guid BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;
    public Guid? UserId { get; set; } // Nullable for guest comments
    public User? User { get; set; }
    public Guid? ParentCommentId { get; set; } // For nested comments/replies
    public BlogComment? ParentComment { get; set; }
    public ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    public string AuthorName { get; set; } = string.Empty; // For guest comments
    public string AuthorEmail { get; set; } = string.Empty; // For guest comments
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public int LikeCount { get; set; } = 0;
}

