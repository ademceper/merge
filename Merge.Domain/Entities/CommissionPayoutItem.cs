namespace Merge.Domain.Entities;

/// <summary>
/// CommissionPayoutItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CommissionPayoutItem : BaseEntity
{
    public Guid PayoutId { get; set; }
    public Guid CommissionId { get; set; }

    // Navigation properties
    public CommissionPayout Payout { get; set; } = null!;
    public SellerCommission Commission { get; set; } = null!;
}

