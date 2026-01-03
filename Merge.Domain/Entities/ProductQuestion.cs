namespace Merge.Domain.Entities;

/// <summary>
/// ProductQuestion Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductQuestion : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public int AnswerCount { get; set; } = 0;
    public int HelpfulCount { get; set; } = 0;
    public bool HasSellerAnswer { get; set; } = false;

    // Navigation properties
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<ProductAnswer> Answers { get; set; } = new List<ProductAnswer>();
    public ICollection<QuestionHelpfulness> HelpfulnessVotes { get; set; } = new List<QuestionHelpfulness>();
}

