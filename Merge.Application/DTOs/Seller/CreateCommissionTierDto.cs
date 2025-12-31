using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public class CreateCommissionTierDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Minimum satış 0 veya daha büyük olmalıdır.")]
    public decimal MinSales { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Maksimum satış 0 veya daha büyük olmalıdır.")]
    public decimal MaxSales { get; set; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "Komisyon oranı 0 ile 100 arasında olmalıdır.")]
    public decimal CommissionRate { get; set; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "Platform ücreti oranı 0 ile 100 arasında olmalıdır.")]
    public decimal PlatformFeeRate { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Priority { get; set; }
}
