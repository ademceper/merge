using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public record CreateCommissionTierDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; init; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Minimum satış 0 veya daha büyük olmalıdır.")]
    public decimal MinSales { get; init; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Maksimum satış 0 veya daha büyük olmalıdır.")]
    public decimal MaxSales { get; init; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "Komisyon oranı 0 ile 100 arasında olmalıdır.")]
    public decimal CommissionRate { get; init; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "Platform ücreti oranı 0 ile 100 arasında olmalıdır.")]
    public decimal PlatformFeeRate { get; init; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Priority { get; init; }
}
