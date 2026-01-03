namespace Merge.Domain.Entities;

/// <summary>
/// SellerCommissionSettings Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SellerCommissionSettings : BaseEntity
{
    public Guid SellerId { get; set; }
    public decimal CustomCommissionRate { get; set; } = 0; // Override default tier rate
    public bool UseCustomRate { get; set; } = false;
    public decimal MinimumPayoutAmount { get; set; } = 100; // Minimum amount to request payout
    public string? PaymentMethod { get; set; } // Bank transfer, PayPal, etc.
    public string? PaymentDetails { get; set; } // Account number, PayPal email, etc.

    // Navigation properties
    public User Seller { get; set; } = null!;
}

