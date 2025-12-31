using Merge.Application.DTOs.Subscription;
namespace Merge.Application.Interfaces.Subscription;

public interface ISubscriptionService
{
    // Subscription Plans
    Task<SubscriptionPlanDto> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto dto);
    Task<SubscriptionPlanDto?> GetSubscriptionPlanByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionPlanDto>> GetAllSubscriptionPlansAsync(bool? isActive = null);
    Task<bool> UpdateSubscriptionPlanAsync(Guid id, UpdateSubscriptionPlanDto dto);
    Task<bool> DeleteSubscriptionPlanAsync(Guid id);
    
    // User Subscriptions
    Task<UserSubscriptionDto> CreateUserSubscriptionAsync(Guid userId, CreateUserSubscriptionDto dto);
    Task<UserSubscriptionDto?> GetUserSubscriptionByIdAsync(Guid id);
    Task<UserSubscriptionDto?> GetUserActiveSubscriptionAsync(Guid userId);
    Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, string? status = null);
    Task<bool> UpdateUserSubscriptionAsync(Guid id, UpdateUserSubscriptionDto dto);
    Task<bool> CancelUserSubscriptionAsync(Guid id, string? reason = null);
    Task<bool> RenewSubscriptionAsync(Guid id);
    Task<bool> SuspendSubscriptionAsync(Guid id);
    Task<bool> ActivateSubscriptionAsync(Guid id);
    
    // Subscription Payments
    Task<SubscriptionPaymentDto> CreateSubscriptionPaymentAsync(Guid userSubscriptionId, decimal amount);
    Task<bool> ProcessPaymentAsync(Guid paymentId, string transactionId);
    Task<bool> FailPaymentAsync(Guid paymentId, string reason);
    Task<IEnumerable<SubscriptionPaymentDto>> GetSubscriptionPaymentsAsync(Guid userSubscriptionId);
    Task<bool> RetryFailedPaymentAsync(Guid paymentId);
    
    // Subscription Usage
    Task<SubscriptionUsageDto> TrackUsageAsync(Guid userSubscriptionId, string feature, int count = 1);
    Task<SubscriptionUsageDto?> GetUsageAsync(Guid userSubscriptionId, string feature);
    Task<IEnumerable<SubscriptionUsageDto>> GetAllUsageAsync(Guid userSubscriptionId);
    Task<bool> CheckUsageLimitAsync(Guid userSubscriptionId, string feature, int requestedCount = 1);
    
    // Analytics
    Task<SubscriptionAnalyticsDto> GetSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<SubscriptionTrendDto>> GetSubscriptionTrendsAsync(DateTime startDate, DateTime endDate);
}

