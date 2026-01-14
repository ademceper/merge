using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Subscription;

public class CreateUserSubscriptionDto
{
    [Required]
    public Guid SubscriptionPlanId { get; set; }
    
    [StringLength(100)]
    public string? PaymentMethodId { get; set; }
    
    public bool AutoRenew { get; set; } = true;
}
