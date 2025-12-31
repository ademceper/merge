using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Subscription;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Services.Subscription;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SubscriptionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // Subscription Plans
    public async Task<SubscriptionPlanDto> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto dto)
    {
        var plan = new SubscriptionPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            PlanType = dto.PlanType,
            Price = dto.Price,
            DurationDays = dto.DurationDays,
            TrialDays = dto.TrialDays,
            Features = dto.Features != null ? JsonSerializer.Serialize(dto.Features) : null,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            BillingCycle = dto.BillingCycle,
            MaxUsers = dto.MaxUsers,
            SetupFee = dto.SetupFee,
            Currency = dto.Currency
        };

        await _context.Set<SubscriptionPlan>().AddAsync(plan);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created subscription plan {PlanName} with ID {PlanId}", plan.Name, plan.Id);

        return await MapToPlanDto(plan);
    }

    public async Task<SubscriptionPlanDto?> GetSubscriptionPlanByIdAsync(Guid id)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return plan != null ? await MapToPlanDto(plan) : null;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> GetAllSubscriptionPlansAsync(bool? isActive = null)
    {
        var query = _context.Set<SubscriptionPlan>()
            .AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var plans = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Price)
            .ToListAsync();

        var result = new List<SubscriptionPlanDto>();
        foreach (var plan in plans)
        {
            result.Add(await MapToPlanDto(plan));
        }
        return result;
    }

    public async Task<bool> UpdateSubscriptionPlanAsync(Guid id, UpdateSubscriptionPlanDto dto)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
            plan.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            plan.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.PlanType))
            plan.PlanType = dto.PlanType;
        if (dto.Price.HasValue)
            plan.Price = dto.Price.Value;
        if (dto.DurationDays.HasValue)
            plan.DurationDays = dto.DurationDays.Value;
        if (dto.TrialDays.HasValue)
            plan.TrialDays = dto.TrialDays;
        if (dto.Features != null)
            plan.Features = JsonSerializer.Serialize(dto.Features);
        if (dto.IsActive.HasValue)
            plan.IsActive = dto.IsActive.Value;
        if (dto.DisplayOrder.HasValue)
            plan.DisplayOrder = dto.DisplayOrder.Value;
        if (!string.IsNullOrEmpty(dto.BillingCycle))
            plan.BillingCycle = dto.BillingCycle;
        if (dto.MaxUsers.HasValue)
            plan.MaxUsers = dto.MaxUsers.Value;
        if (dto.SetupFee.HasValue)
            plan.SetupFee = dto.SetupFee;
        if (!string.IsNullOrEmpty(dto.Currency))
            plan.Currency = dto.Currency;

        plan.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated subscription plan {PlanId} ({PlanName})", plan.Id, plan.Name);

        return true;
    }

    public async Task<bool> DeleteSubscriptionPlanAsync(Guid id)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (plan == null) return false;

        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // User Subscriptions
    public async Task<UserSubscriptionDto> CreateUserSubscriptionAsync(Guid userId, CreateUserSubscriptionDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == dto.SubscriptionPlanId && !p.IsDeleted && p.IsActive);

        if (plan == null)
        {
            throw new NotFoundException("Abonelik planı", dto.SubscriptionPlanId);
        }

        // Check if user already has an active subscription
        var existingActive = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.UserId == userId && us.Status == "Active" && !us.IsDeleted);

        if (existingActive != null)
        {
            throw new BusinessException("Kullanıcının zaten aktif bir aboneliği var.");
        }

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(plan.DurationDays);
        DateTime? trialEndDate = null;

        if (plan.TrialDays.HasValue && plan.TrialDays.Value > 0)
        {
            trialEndDate = startDate.AddDays(plan.TrialDays.Value);
        }

        var subscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = dto.SubscriptionPlanId,
            Status = plan.TrialDays.HasValue && plan.TrialDays.Value > 0 ? "Trial" : "Active",
            StartDate = startDate,
            EndDate = endDate,
            TrialEndDate = trialEndDate,
            AutoRenew = dto.AutoRenew,
            NextBillingDate = trialEndDate ?? endDate,
            CurrentPrice = plan.Price,
            PaymentMethodId = dto.PaymentMethodId
        };

        await _context.Set<UserSubscription>().AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        // Create initial payment if not trial
        if (subscription.Status != "Trial")
        {
            await CreateSubscriptionPaymentAsync(subscription.Id, plan.Price);
        }

        return await MapToUserSubscriptionDto(subscription);
    }

    public async Task<UserSubscriptionDto?> GetUserSubscriptionByIdAsync(Guid id)
    {
        var subscription = await _context.Set<UserSubscription>()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);

        return subscription != null ? await MapToUserSubscriptionDto(subscription) : null;
    }

    public async Task<UserSubscriptionDto?> GetUserActiveSubscriptionAsync(Guid userId)
    {
        var subscription = await _context.Set<UserSubscription>()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.UserId == userId && 
                        (us.Status == "Active" || us.Status == "Trial") && 
                        !us.IsDeleted &&
                        us.EndDate > DateTime.UtcNow)
            .OrderByDescending(us => us.CreatedAt)
            .FirstOrDefaultAsync();

        return subscription != null ? await MapToUserSubscriptionDto(subscription) : null;
    }

    public async Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, string? status = null)
    {
        var query = _context.Set<UserSubscription>()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.UserId == userId && !us.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(us => us.Status == status);
        }

        var subscriptions = await query
            .OrderByDescending(us => us.CreatedAt)
            .ToListAsync();

        var result = new List<UserSubscriptionDto>();
        foreach (var subscription in subscriptions)
        {
            result.Add(await MapToUserSubscriptionDto(subscription));
        }
        return result;
    }

    public async Task<bool> UpdateUserSubscriptionAsync(Guid id, UpdateUserSubscriptionDto dto)
    {
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);

        if (subscription == null) return false;

        if (dto.AutoRenew.HasValue)
            subscription.AutoRenew = dto.AutoRenew.Value;
        if (!string.IsNullOrEmpty(dto.PaymentMethodId))
            subscription.PaymentMethodId = dto.PaymentMethodId;

        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelUserSubscriptionAsync(Guid id, string? reason = null)
    {
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);

        if (subscription == null || subscription.Status == "Cancelled") return false;

        subscription.Status = "Cancelled";
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = reason;
        subscription.AutoRenew = false;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RenewSubscriptionAsync(Guid id)
    {
        var subscription = await _context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);

        if (subscription == null || subscription.Status != "Active") return false;

        var plan = subscription.SubscriptionPlan;
        if (plan == null) return false;

        subscription.EndDate = subscription.EndDate.AddDays(plan.DurationDays);
        subscription.NextBillingDate = subscription.EndDate;
        subscription.RenewalCount++;
        subscription.UpdatedAt = DateTime.UtcNow;

        // Create payment for renewal
        await CreateSubscriptionPaymentAsync(subscription.Id, plan.Price);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendSubscriptionAsync(Guid id)
    {
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);

        if (subscription == null || subscription.Status != "Active") return false;

        subscription.Status = "Suspended";
        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateSubscriptionAsync(Guid id)
    {
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);

        if (subscription == null || subscription.Status != "Suspended") return false;

        subscription.Status = "Active";
        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Subscription Payments
    public async Task<SubscriptionPaymentDto> CreateSubscriptionPaymentAsync(Guid userSubscriptionId, decimal amount)
    {
        var subscription = await _context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == userSubscriptionId && !us.IsDeleted);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", userSubscriptionId);
        }

        var billingPeriodStart = subscription.NextBillingDate ?? subscription.StartDate;
        var billingPeriodEnd = billingPeriodStart.AddDays(subscription.SubscriptionPlan?.DurationDays ?? 30);

        var payment = new SubscriptionPayment
        {
            UserSubscriptionId = userSubscriptionId,
            PaymentStatus = "Pending",
            Amount = amount,
            BillingPeriodStart = billingPeriodStart,
            BillingPeriodEnd = billingPeriodEnd
        };

        await _context.Set<SubscriptionPayment>().AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return await MapToPaymentDto(payment);
    }

    public async Task<bool> ProcessPaymentAsync(Guid paymentId, string transactionId)
    {
        var payment = await _context.Set<SubscriptionPayment>()
            .Include(p => p.UserSubscription)
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

        if (payment == null) return false;

        payment.PaymentStatus = "Completed";
        payment.TransactionId = transactionId;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        // Update subscription if needed
        if (payment.UserSubscription != null && payment.UserSubscription.Status == "Trial")
        {
            payment.UserSubscription.Status = "Active";
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> FailPaymentAsync(Guid paymentId, string reason)
    {
        var payment = await _context.Set<SubscriptionPayment>()
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

        if (payment == null) return false;

        payment.PaymentStatus = "Failed";
        payment.FailureReason = reason;
        payment.RetryCount++;
        payment.NextRetryDate = DateTime.UtcNow.AddDays(1);
        payment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<SubscriptionPaymentDto>> GetSubscriptionPaymentsAsync(Guid userSubscriptionId)
    {
        var payments = await _context.Set<SubscriptionPayment>()
            .Where(p => p.UserSubscriptionId == userSubscriptionId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var result = new List<SubscriptionPaymentDto>();
        foreach (var payment in payments)
        {
            result.Add(await MapToPaymentDto(payment));
        }
        return result;
    }

    public async Task<bool> RetryFailedPaymentAsync(Guid paymentId)
    {
        var payment = await _context.Set<SubscriptionPayment>()
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted && p.PaymentStatus == "Failed");

        if (payment == null) return false;

        payment.PaymentStatus = "Pending";
        payment.RetryCount++;
        payment.NextRetryDate = null;
        payment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Subscription Usage
    public async Task<SubscriptionUsageDto> TrackUsageAsync(Guid userSubscriptionId, string feature, int count = 1)
    {
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == userSubscriptionId && !us.IsDeleted);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", userSubscriptionId);
        }

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var usage = await _context.Set<SubscriptionUsage>()
            .FirstOrDefaultAsync(u => u.UserSubscriptionId == userSubscriptionId &&
                                     u.Feature == feature &&
                                     u.PeriodStart == periodStart &&
                                     !u.IsDeleted);

        if (usage == null)
        {
            usage = new SubscriptionUsage
            {
                UserSubscriptionId = userSubscriptionId,
                Feature = feature,
                UsageCount = count,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            };
            await _context.Set<SubscriptionUsage>().AddAsync(usage);
        }
        else
        {
            usage.UsageCount += count;
        }

        await _unitOfWork.SaveChangesAsync();

        return await MapToUsageDto(usage);
    }

    public async Task<SubscriptionUsageDto?> GetUsageAsync(Guid userSubscriptionId, string feature)
    {
        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var usage = await _context.Set<SubscriptionUsage>()
            .FirstOrDefaultAsync(u => u.UserSubscriptionId == userSubscriptionId &&
                                     u.Feature == feature &&
                                     u.PeriodStart == periodStart &&
                                     !u.IsDeleted);

        return usage != null ? await MapToUsageDto(usage) : null;
    }

    public async Task<IEnumerable<SubscriptionUsageDto>> GetAllUsageAsync(Guid userSubscriptionId)
    {
        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var usages = await _context.Set<SubscriptionUsage>()
            .Where(u => u.UserSubscriptionId == userSubscriptionId &&
                       u.PeriodStart == periodStart &&
                       !u.IsDeleted)
            .ToListAsync();

        var result = new List<SubscriptionUsageDto>();
        foreach (var usage in usages)
        {
            result.Add(await MapToUsageDto(usage));
        }
        return result;
    }

    public async Task<bool> CheckUsageLimitAsync(Guid userSubscriptionId, string feature, int requestedCount = 1)
    {
        var usage = await GetUsageAsync(userSubscriptionId, feature);
        
        if (usage == null) return true; // No limit set or no usage tracked
        
        if (usage.Limit.HasValue)
        {
            return (usage.UsageCount + requestedCount) <= usage.Limit.Value;
        }

        return true; // No limit
    }

    // Analytics
    public async Task<SubscriptionAnalyticsDto> GetSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = endDate ?? DateTime.UtcNow;

        var allSubscriptions = await _context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .Where(us => !us.IsDeleted && us.CreatedAt >= start && us.CreatedAt <= end)
            .ToListAsync();

        var activeSubscriptions = allSubscriptions.Where(us => us.Status == "Active" && us.EndDate > DateTime.UtcNow).ToList();
        var trialSubscriptions = allSubscriptions.Where(us => us.Status == "Trial").ToList();
        var cancelledSubscriptions = allSubscriptions.Where(us => us.Status == "Cancelled").ToList();

        var mrr = activeSubscriptions.Sum(us => us.CurrentPrice);
        var arr = mrr * 12;

        var totalSubscriptions = allSubscriptions.Count;
        var churnRate = totalSubscriptions > 0 
            ? (decimal)cancelledSubscriptions.Count / totalSubscriptions * 100 
            : 0;

        var arpu = activeSubscriptions.Any() 
            ? activeSubscriptions.Average(us => us.CurrentPrice) 
            : 0;

        var subscriptionsByPlan = allSubscriptions
            .GroupBy(us => us.SubscriptionPlan?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var revenueByPlan = allSubscriptions
            .Where(us => us.Status == "Active")
            .GroupBy(us => us.SubscriptionPlan?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Sum(us => us.CurrentPrice));

        var trends = await GetSubscriptionTrendsAsync(start, end);

        return new SubscriptionAnalyticsDto
        {
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptions.Count,
            TrialSubscriptions = trialSubscriptions.Count,
            CancelledSubscriptions = cancelledSubscriptions.Count,
            MonthlyRecurringRevenue = mrr,
            AnnualRecurringRevenue = arr,
            ChurnRate = churnRate,
            AverageRevenuePerUser = arpu,
            SubscriptionsByPlan = subscriptionsByPlan,
            RevenueByPlan = revenueByPlan,
            Trends = trends.ToList()
        };
    }

    public async Task<IEnumerable<SubscriptionTrendDto>> GetSubscriptionTrendsAsync(DateTime startDate, DateTime endDate)
    {
        var subscriptions = await _context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .Where(us => !us.IsDeleted && us.CreatedAt >= startDate && us.CreatedAt <= endDate)
            .ToListAsync();

        var payments = await _context.Set<SubscriptionPayment>()
            .Where(p => !p.IsDeleted && p.CreatedAt >= startDate && p.CreatedAt <= endDate && p.PaymentStatus == "Completed")
            .ToListAsync();

        var trends = new List<SubscriptionTrendDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthSubscriptions = subscriptions
                .Where(us => us.CreatedAt >= monthStart && us.CreatedAt <= monthEnd)
                .ToList();

            var monthCancellations = monthSubscriptions
                .Where(us => us.Status == "Cancelled" && us.CancelledAt >= monthStart && us.CancelledAt <= monthEnd)
                .Count();

            var activeAtMonthEnd = subscriptions
                .Where(us => us.Status == "Active" && us.EndDate > monthEnd)
                .Count();

            var monthRevenue = payments
                .Where(p => p.PaidAt >= monthStart && p.PaidAt <= monthEnd)
                .Sum(p => p.Amount);

            trends.Add(new SubscriptionTrendDto
            {
                Date = monthStart,
                NewSubscriptions = monthSubscriptions.Count,
                Cancellations = monthCancellations,
                ActiveSubscriptions = activeAtMonthEnd,
                Revenue = monthRevenue
            });

            currentDate = monthStart.AddMonths(1);
        }

        return trends;
    }

    // Helper methods
    private async Task<SubscriptionPlanDto> MapToPlanDto(SubscriptionPlan plan)
    {
        var subscriberCount = await _context.Set<UserSubscription>()
            .CountAsync(us => us.SubscriptionPlanId == plan.Id && 
                            (us.Status == "Active" || us.Status == "Trial") && 
                            !us.IsDeleted);

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            PlanType = plan.PlanType,
            Price = plan.Price,
            DurationDays = plan.DurationDays,
            TrialDays = plan.TrialDays,
            Features = !string.IsNullOrEmpty(plan.Features)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(plan.Features)
                : null,
            IsActive = plan.IsActive,
            DisplayOrder = plan.DisplayOrder,
            BillingCycle = plan.BillingCycle,
            MaxUsers = plan.MaxUsers,
            SetupFee = plan.SetupFee,
            Currency = plan.Currency,
            SubscriberCount = subscriberCount,
            CreatedAt = plan.CreatedAt
        };
    }

    private async Task<UserSubscriptionDto> MapToUserSubscriptionDto(UserSubscription subscription)
    {
        await _context.Entry(subscription)
            .Reference(us => us.User)
            .LoadAsync();
        await _context.Entry(subscription)
            .Reference(us => us.SubscriptionPlan)
            .LoadAsync();

        var recentPayments = await _context.Set<SubscriptionPayment>()
            .Where(p => p.UserSubscriptionId == subscription.Id && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        var daysRemaining = subscription.EndDate > DateTime.UtcNow
            ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
            : 0;

        return new UserSubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            UserName = subscription.User != null
                ? $"{subscription.User.FirstName} {subscription.User.LastName}"
                : string.Empty,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            PlanName = subscription.SubscriptionPlan?.Name ?? string.Empty,
            Status = subscription.Status,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            TrialEndDate = subscription.TrialEndDate,
            IsTrial = subscription.Status == "Trial",
            CancelledAt = subscription.CancelledAt,
            CancellationReason = subscription.CancellationReason,
            AutoRenew = subscription.AutoRenew,
            NextBillingDate = subscription.NextBillingDate,
            CurrentPrice = subscription.CurrentPrice,
            RenewalCount = subscription.RenewalCount,
            DaysRemaining = daysRemaining,
            RecentPayments = recentPayments.Select(p => new SubscriptionPaymentDto
            {
                Id = p.Id,
                UserSubscriptionId = p.UserSubscriptionId,
                PaymentStatus = p.PaymentStatus,
                Amount = p.Amount,
                TransactionId = p.TransactionId,
                PaidAt = p.PaidAt,
                BillingPeriodStart = p.BillingPeriodStart,
                BillingPeriodEnd = p.BillingPeriodEnd,
                FailureReason = p.FailureReason,
                RetryCount = p.RetryCount,
                NextRetryDate = p.NextRetryDate,
                CreatedAt = p.CreatedAt
            }).ToList(),
            CreatedAt = subscription.CreatedAt
        };
    }

    private Task<SubscriptionPaymentDto> MapToPaymentDto(SubscriptionPayment payment)
    {
        return Task.FromResult(new SubscriptionPaymentDto
        {
            Id = payment.Id,
            UserSubscriptionId = payment.UserSubscriptionId,
            PaymentStatus = payment.PaymentStatus,
            Amount = payment.Amount,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            BillingPeriodStart = payment.BillingPeriodStart,
            BillingPeriodEnd = payment.BillingPeriodEnd,
            FailureReason = payment.FailureReason,
            RetryCount = payment.RetryCount,
            NextRetryDate = payment.NextRetryDate,
            CreatedAt = payment.CreatedAt
        });
    }

    private Task<SubscriptionUsageDto> MapToUsageDto(SubscriptionUsage usage)
    {
        return Task.FromResult(new SubscriptionUsageDto
        {
            Id = usage.Id,
            UserSubscriptionId = usage.UserSubscriptionId,
            Feature = usage.Feature,
            UsageCount = usage.UsageCount,
            Limit = usage.Limit,
            Remaining = usage.Limit.HasValue ? usage.Limit.Value - usage.UsageCount : null,
            PeriodStart = usage.PeriodStart,
            PeriodEnd = usage.PeriodEnd
        });
    }
}

