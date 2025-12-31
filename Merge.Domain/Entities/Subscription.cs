namespace Merge.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty; // Monthly, Quarterly, Yearly, Lifetime
    public decimal Price { get; set; }
    public int DurationDays { get; set; } // How many days the subscription lasts
    public int? TrialDays { get; set; } // Free trial period
    public string? Features { get; set; } // JSON string for plan features
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public string? BillingCycle { get; set; } // Monthly, Quarterly, Yearly
    public int MaxUsers { get; set; } = 1; // Maximum users allowed
    public decimal? SetupFee { get; set; } // One-time setup fee
    public string? Currency { get; set; } = "TRY";
    
    // Navigation properties
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string Status { get; set; } = "Active"; // Active, Cancelled, Expired, Suspended, Trial
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public bool AutoRenew { get; set; } = true;
    public DateTime? NextBillingDate { get; set; }
    public decimal CurrentPrice { get; set; } // Price at time of subscription
    public string? PaymentMethodId { get; set; } // Reference to payment method
    public int RenewalCount { get; set; } = 0; // How many times renewed
    
    // Navigation properties
    public User User { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
}

public class SubscriptionPayment : BaseEntity
{
    public Guid UserSubscriptionId { get; set; }
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; } // External payment gateway transaction ID
    public DateTime? PaidAt { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime? NextRetryDate { get; set; }
    
    // Navigation properties
    public UserSubscription UserSubscription { get; set; } = null!;
}

public class SubscriptionUsage : BaseEntity
{
    public Guid UserSubscriptionId { get; set; }
    public string Feature { get; set; } = string.Empty; // Feature name being used
    public int UsageCount { get; set; } = 0;
    public int? Limit { get; set; } // Usage limit for this feature
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    // Navigation properties
    public UserSubscription UserSubscription { get; set; } = null!;
}

