namespace Merge.Application.DTOs.Seller;

public class SellerCommissionSettingsDto
{
    public Guid SellerId { get; set; }
    public decimal CustomCommissionRate { get; set; }
    public bool UseCustomRate { get; set; }
    public decimal MinimumPayoutAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentDetails { get; set; }
}
