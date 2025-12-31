using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public class UpdateCommissionSettingsDto
{
    [Range(0, 100, ErrorMessage = "Komisyon oranı 0 ile 100 arasında olmalıdır.")]
    public decimal? CustomCommissionRate { get; set; }
    
    public bool? UseCustomRate { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Minimum ödeme tutarı 0 veya daha büyük olmalıdır.")]
    public decimal? MinimumPayoutAmount { get; set; }
    
    [StringLength(50)]
    public string? PaymentMethod { get; set; }
    
    [StringLength(500)]
    public string? PaymentDetails { get; set; }
}
