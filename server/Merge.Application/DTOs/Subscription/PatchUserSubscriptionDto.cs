namespace Merge.Application.DTOs.Subscription;

/// <summary>
/// Partial update DTO for User Subscription (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchUserSubscriptionDto
{
    public bool? AutoRenew { get; init; }
    public string? PaymentMethodId { get; init; }
}
