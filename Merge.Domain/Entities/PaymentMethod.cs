namespace Merge.Domain.Entities;

public class PaymentMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Credit Card, Bank Transfer, Cash on Delivery, etc.
    public string Code { get; set; } = string.Empty; // CC, BT, COD, etc.
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool RequiresOnlinePayment { get; set; } = false; // For gateway integration
    public bool RequiresManualVerification { get; set; } = false; // For bank transfer, etc.
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public decimal? ProcessingFee { get; set; } // Fixed fee
    public decimal? ProcessingFeePercentage { get; set; } // Percentage fee
    public string? Settings { get; set; } // JSON for method-specific settings
    public int DisplayOrder { get; set; } = 0;
    public bool IsDefault { get; set; } = false;
}

