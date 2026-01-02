using Merge.Application.DTOs.Subscription;
namespace Merge.Application.Interfaces.Subscription;

public interface ISubscriptionService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // Subscription Plans
    Task<SubscriptionPlanDto> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto dto, CancellationToken cancellationToken = default);
    Task<SubscriptionPlanDto?> GetSubscriptionPlanByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionPlanDto>> GetAllSubscriptionPlansAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateSubscriptionPlanAsync(Guid id, UpdateSubscriptionPlanDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSubscriptionPlanAsync(Guid id, CancellationToken cancellationToken = default);
    
    // User Subscriptions
    Task<UserSubscriptionDto> CreateUserSubscriptionAsync(Guid userId, CreateUserSubscriptionDto dto, CancellationToken cancellationToken = default);
    Task<UserSubscriptionDto?> GetUserSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserSubscriptionDto?> GetUserActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, string? status = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserSubscriptionAsync(Guid id, UpdateUserSubscriptionDto dto, CancellationToken cancellationToken = default);
    Task<bool> CancelUserSubscriptionAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default);
    Task<bool> RenewSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SuspendSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ActivateSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Subscription Payments
    Task<SubscriptionPaymentDto> CreateSubscriptionPaymentAsync(Guid userSubscriptionId, decimal amount, CancellationToken cancellationToken = default);
    Task<bool> ProcessPaymentAsync(Guid paymentId, string transactionId, CancellationToken cancellationToken = default);
    Task<bool> FailPaymentAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionPaymentDto>> GetSubscriptionPaymentsAsync(Guid userSubscriptionId, CancellationToken cancellationToken = default);
    Task<bool> RetryFailedPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    
    // Subscription Usage
    Task<SubscriptionUsageDto> TrackUsageAsync(Guid userSubscriptionId, string feature, int count = 1, CancellationToken cancellationToken = default);
    Task<SubscriptionUsageDto?> GetUsageAsync(Guid userSubscriptionId, string feature, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionUsageDto>> GetAllUsageAsync(Guid userSubscriptionId, CancellationToken cancellationToken = default);
    Task<bool> CheckUsageLimitAsync(Guid userSubscriptionId, string feature, int requestedCount = 1, CancellationToken cancellationToken = default);
    
    // Analytics
    Task<SubscriptionAnalyticsDto> GetSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionTrendDto>> GetSubscriptionTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

