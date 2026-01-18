using Merge.Domain.Modules.Payment;
namespace Merge.Application.DTOs.Seller;

public record SellerCommissionSettingsDto
{
    public Guid SellerId { get; init; }
    public decimal CustomCommissionRate { get; init; }
    public bool UseCustomRate { get; init; }
    public decimal MinimumPayoutAmount { get; init; }
    public string? PaymentMethod { get; init; }
    public string? PaymentDetails { get; init; }
}
