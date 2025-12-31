using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Subscription;

public class UpdateUserSubscriptionDto
{
    public bool? AutoRenew { get; set; }
    
    [StringLength(100)]
    public string? PaymentMethodId { get; set; }
}
