using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Payment;

public class CreatePaymentMethodDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Kod en az 2, en fazla 50 karakter olmalıdır.")]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string IconUrl { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public bool RequiresOnlinePayment { get; set; } = false;
    
    public bool RequiresManualVerification { get; set; } = false;
    
    [Range(0, double.MaxValue, ErrorMessage = "Minimum tutar 0 veya daha büyük olmalıdır.")]
    public decimal? MinimumAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Maksimum tutar 0 veya daha büyük olmalıdır.")]
    public decimal? MaximumAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "İşlem ücreti 0 veya daha büyük olmalıdır.")]
    public decimal? ProcessingFee { get; set; }
    
    [Range(0, 100, ErrorMessage = "İşlem ücreti yüzdesi 0 ile 100 arasında olmalıdır.")]
    public decimal? ProcessingFeePercentage { get; set; }

    /// <summary>
    /// Odeme yontemi ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public PaymentMethodSettingsDto? Settings { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsDefault { get; set; } = false;
}
