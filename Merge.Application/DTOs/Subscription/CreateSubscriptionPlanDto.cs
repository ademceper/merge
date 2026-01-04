using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Subscription;

public class CreateSubscriptionPlanDto
{
    [Required(ErrorMessage = "Plan adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Plan adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
    [Required(ErrorMessage = "Plan tipi zorunludur.")]
    public SubscriptionPlanType PlanType { get; set; } = SubscriptionPlanType.Monthly;

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Süre zorunludur.")]
    [Range(1, int.MaxValue, ErrorMessage = "Süre en az 1 gün olmalıdır.")]
    public int DurationDays { get; set; }

    [Range(0, int.MaxValue)]
    public int? TrialDays { get; set; }

    /// Typed DTO (Over-posting korumasi)
    public SubscriptionPlanFeaturesDto? Features { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;

    // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    [Range(1, int.MaxValue, ErrorMessage = "Maksimum kullanıcı sayısı en az 1 olmalıdır.")]
    public int MaxUsers { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "Kurulum ücreti 0 veya daha büyük olmalıdır.")]
    public decimal? SetupFee { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; } = "TRY";
}
