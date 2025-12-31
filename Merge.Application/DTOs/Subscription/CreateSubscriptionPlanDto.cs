using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Subscription;

public class CreateSubscriptionPlanDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Plan adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string PlanType { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal Price { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Süre en az 1 gün olmalıdır.")]
    public int DurationDays { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? TrialDays { get; set; }
    
    public Dictionary<string, object>? Features { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    [StringLength(50)]
    public string? BillingCycle { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Maksimum kullanıcı sayısı en az 1 olmalıdır.")]
    public int MaxUsers { get; set; } = 1;
    
    [Range(0, double.MaxValue, ErrorMessage = "Kurulum ücreti 0 veya daha büyük olmalıdır.")]
    public decimal? SetupFee { get; set; }
    
    [StringLength(10)]
    public string? Currency { get; set; } = "TRY";
}
