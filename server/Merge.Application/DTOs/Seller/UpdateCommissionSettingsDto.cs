using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Seller;

public record UpdateCommissionSettingsDto
{
    [Range(0, 100, ErrorMessage = "Komisyon oranı 0 ile 100 arasında olmalıdır.")]
    public decimal? CustomCommissionRate { get; init; }
    
    public bool? UseCustomRate { get; init; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Minimum ödeme tutarı 0 veya daha büyük olmalıdır.")]
    public decimal? MinimumPayoutAmount { get; init; }
    
    [StringLength(50)]
    public string? PaymentMethod { get; init; }
    
    [StringLength(500)]
    public string? PaymentDetails { get; init; }
}
