using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Subscription;

public class UpdateSubscriptionPlanDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Plan adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public SubscriptionPlanType? PlanType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Süre en az 1 gün olmalıdır.")]
    public int? DurationDays { get; set; }

    [Range(0, int.MaxValue)]
    public int? TrialDays { get; set; }

    /// Typed DTO (Over-posting korumasi)
    public SubscriptionPlanFeaturesDto? Features { get; set; }

    public bool? IsActive { get; set; }

    [Range(0, int.MaxValue)]
    public int? DisplayOrder { get; set; }

    public BillingCycle? BillingCycle { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Maksimum kullanıcı sayısı en az 1 olmalıdır.")]
    public int? MaxUsers { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Kurulum ücreti 0 veya daha büyük olmalıdır.")]
    public decimal? SetupFee { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }
}
