namespace Merge.Domain.Entities;

/// <summary>
/// SellerDocument Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SellerDocument : BaseEntity
{
    public Guid SellerApplicationId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // Identity, Tax, Bank, License
    public string DocumentUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }

    // Navigation properties
    public SellerApplication SellerApplication { get; set; } = null!;
}

