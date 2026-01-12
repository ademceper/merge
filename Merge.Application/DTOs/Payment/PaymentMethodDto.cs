using Merge.Domain.Modules.Payment;
namespace Merge.Application.DTOs.Payment;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool RequiresOnlinePayment { get; set; }
    public bool RequiresManualVerification { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public decimal? ProcessingFee { get; set; }
    public decimal? ProcessingFeePercentage { get; set; }
    /// <summary>
    /// Odeme yontemi ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public PaymentMethodSettingsDto? Settings { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
