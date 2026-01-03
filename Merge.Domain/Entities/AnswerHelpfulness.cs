namespace Merge.Domain.Entities;

/// <summary>
/// AnswerHelpfulness Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class AnswerHelpfulness : BaseEntity
{
    public Guid AnswerId { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public ProductAnswer Answer { get; set; } = null!;
    public User User { get; set; } = null!;
}

