namespace Merge.Domain.Entities;

/// <summary>
/// QuestionHelpfulness Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class QuestionHelpfulness : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public ProductQuestion Question { get; set; } = null!;
    public User User { get; set; } = null!;
}

