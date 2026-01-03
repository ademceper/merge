using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Subscription;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Services.Subscription;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SubscriptionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // Subscription Plans
    public async Task<SubscriptionPlanDto> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto dto, CancellationToken cancellationToken = default)
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

        await _context.Set<SubscriptionPlan>().AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created subscription plan {PlanName} with ID {PlanId}", plan.Name, plan.Id);

        // ✅ PERFORMANCE: Batch load subscriber count for all plans
        var subscriberCount = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .CountAsync(us => us.SubscriptionPlanId == plan.Id && 
                            (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial));

        var planDto = _mapper.Map<SubscriptionPlanDto>(plan);
        planDto.SubscriberCount = subscriberCount;
        return planDto;
    }

    public async Task<SubscriptionPlanDto?> GetSubscriptionPlanByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan == null) return null;

        // ✅ PERFORMANCE: Batch load subscriber count
        var subscriberCount = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .CountAsync(us => us.SubscriptionPlanId == plan.Id && 
                            (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial));

        var dto = _mapper.Map<SubscriptionPlanDto>(plan);
        dto.SubscriberCount = subscriberCount;
        return dto;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> GetAllSubscriptionPlansAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<SubscriptionPlan>()
            .AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // ✅ PERFORMANCE: planIds'i database'de oluştur, memory'de işlem YASAK
        var planIds = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Price)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var plans = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Price)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load subscriber counts for all plans
        var subscriberCounts = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .Where(us => planIds.Contains(us.SubscriptionPlanId) && 
                        (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial))
            .GroupBy(us => us.SubscriptionPlanId)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanId, x => x.Count);

        var result = new List<SubscriptionPlanDto>();
        foreach (var plan in plans)
        {
            var dto = _mapper.Map<SubscriptionPlanDto>(plan);
            dto.SubscriberCount = subscriberCounts.TryGetValue(plan.Id, out var count) ? count : 0;
            result.Add(dto);
        }
        return result;
    }

    public async Task<bool> UpdateSubscriptionPlanAsync(Guid id, UpdateSubscriptionPlanDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
            plan.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            plan.Description = dto.Description;
        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        if (dto.PlanType.HasValue)
            plan.PlanType = dto.PlanType.Value;
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
        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        if (dto.BillingCycle.HasValue)
            plan.BillingCycle = dto.BillingCycle.Value;
        if (dto.MaxUsers.HasValue)
            plan.MaxUsers = dto.MaxUsers.Value;
        if (dto.SetupFee.HasValue)
            plan.SetupFee = dto.SetupFee;
        if (!string.IsNullOrEmpty(dto.Currency))
            plan.Currency = dto.Currency;

        plan.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated subscription plan {PlanId} ({PlanName})", plan.Id, plan.Name);

        return true;
    }

    public async Task<bool> DeleteSubscriptionPlanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (delete için)
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan == null) return false;

        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // User Subscriptions
    public async Task<UserSubscriptionDto> CreateUserSubscriptionAsync(Guid userId, CreateUserSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var plan = await _context.Set<SubscriptionPlan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.SubscriptionPlanId && p.IsActive, cancellationToken);

        if (plan == null)
        {
            throw new NotFoundException("Abonelik planı", dto.SubscriptionPlanId);
        }

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // Check if user already has an active subscription
        var existingActive = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .FirstOrDefaultAsync(us => us.UserId == userId && us.Status == SubscriptionStatus.Active, cancellationToken);

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
            Status = plan.TrialDays.HasValue && plan.TrialDays.Value > 0 ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
            StartDate = startDate,
            EndDate = endDate,
            TrialEndDate = trialEndDate,
            AutoRenew = dto.AutoRenew,
            NextBillingDate = trialEndDate ?? endDate,
            CurrentPrice = plan.Price,
            PaymentMethodId = dto.PaymentMethodId
        };

        await _context.Set<UserSubscription>().AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create initial payment if not trial
        if (subscription.Status != SubscriptionStatus.Trial)
        {
            await CreateSubscriptionPaymentAsync(subscription.Id, plan.Price);
        }

        // ✅ PERFORMANCE: Reload with includes for mapping
        subscription = await _context.Set<UserSubscription>()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == subscription.Id, cancellationToken);

        return await MapToUserSubscriptionDtoAsync(subscription!, cancellationToken);
    }

    public async Task<UserSubscriptionDto?> GetUserSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var subscription = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == id, cancellationToken);

        return subscription != null ? await MapToUserSubscriptionDtoAsync(subscription, cancellationToken) : null;
    }

    public async Task<UserSubscriptionDto?> GetUserActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var subscription = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.UserId == userId && 
                        (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial) && 
                        us.EndDate > DateTime.UtcNow)
            .OrderByDescending(us => us.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return subscription != null ? await MapToUserSubscriptionDtoAsync(subscription, cancellationToken) : null;
    }

    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    public async Task<IEnumerable<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var query = _context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.UserId == userId);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<SubscriptionStatus>(status);
            query = query.Where(us => us.Status == statusEnum);
        }

        // ✅ PERFORMANCE: Pagination uygula
        var subscriptionIds = await query
            .OrderByDescending(us => us.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(us => us.Id)
            .ToListAsync(cancellationToken);

        var subscriptions = await query
            .OrderByDescending(us => us.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load recent payments for all subscriptions
        var recentPaymentsDict = await _context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => subscriptionIds.Contains(p.UserSubscriptionId))
            .OrderByDescending(p => p.CreatedAt)
            .GroupBy(p => p.UserSubscriptionId)
            .Select(g => new
            {
                UserSubscriptionId = g.Key,
                Payments = g.Take(5).ToList()
            })
            .ToDictionaryAsync(x => x.UserSubscriptionId, x => x.Payments);

        var result = new List<UserSubscriptionDto>();
        foreach (var subscription in subscriptions)
        {
            var dto = _mapper.Map<UserSubscriptionDto>(subscription);
            dto.DaysRemaining = subscription.EndDate > DateTime.UtcNow
                ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
                : 0;
            
            if (recentPaymentsDict.TryGetValue(subscription.Id, out var payments))
            {
                dto.RecentPayments = _mapper.Map<List<SubscriptionPaymentDto>>(payments);
            }
            else
            {
                dto.RecentPayments = new List<SubscriptionPaymentDto>();
            }
            
            result.Add(dto);
        }
        return result;
    }

    public async Task<bool> UpdateUserSubscriptionAsync(Guid id, UpdateUserSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id, cancellationToken);

        if (subscription == null) return false;

        if (dto.AutoRenew.HasValue)
            subscription.AutoRenew = dto.AutoRenew.Value;
        if (!string.IsNullOrEmpty(dto.PaymentMethodId))
            subscription.PaymentMethodId = dto.PaymentMethodId;

        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelUserSubscriptionAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default)
    {
        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id, cancellationToken);

        if (subscription == null || subscription.Status == SubscriptionStatus.Cancelled) return false;

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = reason;
        subscription.AutoRenew = false;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RenewSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var subscription = await _context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == id, cancellationToken);

        if (subscription == null || subscription.Status != SubscriptionStatus.Active) return false;

        var plan = subscription.SubscriptionPlan;
        if (plan == null) return false;

        subscription.EndDate = subscription.EndDate.AddDays(plan.DurationDays);
        subscription.NextBillingDate = subscription.EndDate;
        subscription.RenewalCount++;
        subscription.UpdatedAt = DateTime.UtcNow;

        // Create payment for renewal
        await CreateSubscriptionPaymentAsync(subscription.Id, plan.Price);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SuspendSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id, cancellationToken);

        if (subscription == null || subscription.Status != SubscriptionStatus.Active) return false;

        subscription.Status = SubscriptionStatus.Suspended;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ActivateSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == id, cancellationToken);

        if (subscription == null || subscription.Status != SubscriptionStatus.Suspended) return false;

        subscription.Status = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Subscription Payments
    public async Task<SubscriptionPaymentDto> CreateSubscriptionPaymentAsync(Guid userSubscriptionId, decimal amount, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var subscription = await _context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == userSubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", userSubscriptionId);
        }

        var billingPeriodStart = subscription.NextBillingDate ?? subscription.StartDate;
        var billingPeriodEnd = billingPeriodStart.AddDays(subscription.SubscriptionPlan?.DurationDays ?? 30);

        var payment = new SubscriptionPayment
        {
            UserSubscriptionId = userSubscriptionId,
            PaymentStatus = PaymentStatus.Pending,
            Amount = amount,
            BillingPeriodStart = billingPeriodStart,
            BillingPeriodEnd = billingPeriodEnd
        };

        await _context.Set<SubscriptionPayment>().AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<SubscriptionPaymentDto>(payment);
    }

    public async Task<bool> ProcessPaymentAsync(Guid paymentId, string transactionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var payment = await _context.Set<SubscriptionPayment>()
            .Include(p => p.UserSubscription)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null) return false;

        payment.PaymentStatus = PaymentStatus.Completed;
        payment.TransactionId = transactionId;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        // Update subscription if needed
        if (payment.UserSubscription != null && payment.UserSubscription.Status == SubscriptionStatus.Trial)
        {
            payment.UserSubscription.Status = SubscriptionStatus.Active;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> FailPaymentAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var payment = await _context.Set<SubscriptionPayment>()
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null) return false;

        payment.PaymentStatus = PaymentStatus.Failed;
        payment.FailureReason = reason;
        payment.RetryCount++;
        payment.NextRetryDate = DateTime.UtcNow.AddDays(1);
        payment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<SubscriptionPaymentDto>> GetSubscriptionPaymentsAsync(Guid userSubscriptionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var payments = await _context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => p.UserSubscriptionId == userSubscriptionId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<SubscriptionPaymentDto>>(payments);
    }

    public async Task<bool> RetryFailedPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var payment = await _context.Set<SubscriptionPayment>()
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.PaymentStatus == PaymentStatus.Failed, cancellationToken);

        if (payment == null) return false;

        payment.PaymentStatus = PaymentStatus.Pending;
        payment.RetryCount++;
        payment.NextRetryDate = null;
        payment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Subscription Usage
    public async Task<SubscriptionUsageDto> TrackUsageAsync(Guid userSubscriptionId, string feature, int count = 1, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var subscription = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .FirstOrDefaultAsync(us => us.Id == userSubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", userSubscriptionId);
        }

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var usage = await _context.Set<SubscriptionUsage>()
            .FirstOrDefaultAsync(u => u.UserSubscriptionId == userSubscriptionId &&
                                     u.Feature == feature &&
                                     u.PeriodStart == periodStart);

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
            await _context.Set<SubscriptionUsage>().AddAsync(usage, cancellationToken);
        }
        else
        {
            usage.UsageCount += count;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<SubscriptionUsageDto>(usage);
    }

    public async Task<SubscriptionUsageDto?> GetUsageAsync(Guid userSubscriptionId, string feature, CancellationToken cancellationToken = default)
    {
        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var usage = await _context.Set<SubscriptionUsage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserSubscriptionId == userSubscriptionId &&
                                     u.Feature == feature &&
                                     u.PeriodStart == periodStart);

        if (usage == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<SubscriptionUsageDto>(usage);
    }

    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    public async Task<IEnumerable<SubscriptionUsageDto>> GetAllUsageAsync(Guid userSubscriptionId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var usages = await _context.Set<SubscriptionUsage>()
            .AsNoTracking()
            .Where(u => u.UserSubscriptionId == userSubscriptionId &&
                       u.PeriodStart == periodStart)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<SubscriptionUsageDto>>(usages);
    }

    public async Task<bool> CheckUsageLimitAsync(Guid userSubscriptionId, string feature, int requestedCount = 1, CancellationToken cancellationToken = default)
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
    public async Task<SubscriptionAnalyticsDto> GetSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = endDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var query = _context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.CreatedAt >= start && us.CreatedAt <= end);

        var totalSubscriptions = await query.CountAsync(cancellationToken);
        var activeSubscriptionsCount = await query.CountAsync(us => us.Status == SubscriptionStatus.Active && us.EndDate > DateTime.UtcNow);
        var trialSubscriptionsCount = await query.CountAsync(us => us.Status == SubscriptionStatus.Trial);
        var cancelledSubscriptionsCount = await query.CountAsync(us => us.Status == SubscriptionStatus.Cancelled);

        var mrr = await query
            .Where(us => us.Status == SubscriptionStatus.Active && us.EndDate > DateTime.UtcNow)
            .SumAsync(us => (decimal?)us.CurrentPrice) ?? 0;
        var arr = mrr * 12;

        var churnRate = totalSubscriptions > 0 
            ? (decimal)cancelledSubscriptionsCount / totalSubscriptions * 100 
            : 0;

        var arpu = activeSubscriptionsCount > 0
            ? await query
                .Where(us => us.Status == SubscriptionStatus.Active && us.EndDate > DateTime.UtcNow)
                .AverageAsync(us => (decimal?)us.CurrentPrice) ?? 0
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var subscriptionsByPlan = await query
            .GroupBy(us => us.SubscriptionPlan != null ? us.SubscriptionPlan.Name : "Unknown")
            .Select(g => new { PlanName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanName, x => x.Count);

        var revenueByPlan = await query
            .Where(us => us.Status == SubscriptionStatus.Active)
            .GroupBy(us => us.SubscriptionPlan != null ? us.SubscriptionPlan.Name : "Unknown")
            .Select(g => new { PlanName = g.Key, Revenue = g.Sum(us => us.CurrentPrice) })
            .ToDictionaryAsync(x => x.PlanName, x => x.Revenue);

        var trends = await GetSubscriptionTrendsAsync(start, end);

        return new SubscriptionAnalyticsDto
        {
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptionsCount,
            TrialSubscriptions = trialSubscriptionsCount,
            CancelledSubscriptions = cancelledSubscriptionsCount,
            MonthlyRecurringRevenue = mrr,
            AnnualRecurringRevenue = arr,
            ChurnRate = churnRate,
            AverageRevenuePerUser = arpu,
            SubscriptionsByPlan = subscriptionsByPlan,
            RevenueByPlan = revenueByPlan,
            Trends = trends.ToList()
        };
    }

    public async Task<IEnumerable<SubscriptionTrendDto>> GetSubscriptionTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var trends = new List<SubscriptionTrendDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // ✅ PERFORMANCE: Database'de count/sum yap
            var monthSubscriptionsCount = await _context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.CreatedAt >= monthStart && us.CreatedAt <= monthEnd);

            var monthCancellations = await _context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.Status == SubscriptionStatus.Cancelled && 
                                 us.CancelledAt.HasValue &&
                                 us.CancelledAt >= monthStart && 
                                 us.CancelledAt <= monthEnd);

            var activeAtMonthEnd = await _context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.Status == SubscriptionStatus.Active && us.EndDate > monthEnd);

            var monthRevenue = await _context.Set<SubscriptionPayment>()
                .AsNoTracking()
                .Where(p => p.PaymentStatus == PaymentStatus.Completed &&
                           p.PaidAt.HasValue &&
                           p.PaidAt >= monthStart && 
                           p.PaidAt <= monthEnd)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            trends.Add(new SubscriptionTrendDto
            {
                Date = monthStart,
                NewSubscriptions = monthSubscriptionsCount,
                Cancellations = monthCancellations,
                ActiveSubscriptions = activeAtMonthEnd,
                Revenue = monthRevenue
            });

            currentDate = monthStart.AddMonths(1);
        }

        return trends;
    }

    // Helper method for UserSubscriptionDto with RecentPayments
    private async Task<UserSubscriptionDto> MapToUserSubscriptionDtoAsync(UserSubscription subscription, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Batch load recent payments (N+1 query fix)
        var recentPayments = await _context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => p.UserSubscriptionId == subscription.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var dto = _mapper.Map<UserSubscriptionDto>(subscription);
        dto.DaysRemaining = subscription.EndDate > DateTime.UtcNow
            ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
            : 0;
        dto.RecentPayments = _mapper.Map<List<SubscriptionPaymentDto>>(recentPayments);
        
        return dto;
    }
}

