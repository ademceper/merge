namespace Merge.Domain.Entities;

/// <summary>
/// ProductAnswer Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductAnswer : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public bool IsSellerAnswer { get; set; } = false;
    public bool IsVerifiedPurchase { get; set; } = false;
    public int HelpfulCount { get; set; } = 0;

    // Navigation properties
    public ProductQuestion Question { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<AnswerHelpfulness> HelpfulnessVotes { get; set; } = new List<AnswerHelpfulness>();
}

