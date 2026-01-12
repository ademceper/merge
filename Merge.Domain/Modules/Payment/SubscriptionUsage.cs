using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// SubscriptionUsage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionUsage : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserSubscriptionId { get; private set; }
    public string Feature { get; private set; } = string.Empty; // Feature name being used
    public int UsageCount { get; private set; } = 0;
    public int? Limit { get; private set; } // Usage limit for this feature
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    
    // Navigation properties
    public UserSubscription UserSubscription { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SubscriptionUsage() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SubscriptionUsage Create(
        UserSubscription subscription,
        string feature,
        DateTime periodStart,
        DateTime periodEnd,
        int? limit = null)
    {
        Guard.AgainstNull(subscription, nameof(subscription));
        Guard.AgainstNullOrEmpty(feature, nameof(feature));

        if (periodStart >= periodEnd)
            throw new DomainException("Period start date must be before end date");

        if (limit.HasValue && limit.Value < 0)
            throw new ArgumentException("Limit cannot be negative", nameof(limit));

        var usage = new SubscriptionUsage
        {
            Id = Guid.NewGuid(),
            UserSubscriptionId = subscription.Id,
            UserSubscription = subscription,
            Feature = feature,
            UsageCount = 0,
            Limit = limit,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            CreatedAt = DateTime.UtcNow
        };

        return usage;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment usage
    public void IncrementUsage(int count = 1)
    {
        Guard.AgainstNegativeOrZero(count, nameof(count));

        if (Limit.HasValue && UsageCount + count > Limit.Value)
            throw new DomainException($"Kullanım limiti aşıldı. Limit: {Limit.Value}, Mevcut: {UsageCount}, İstenen: {count}");

        var wasLimitReached = IsLimitReached();
        UsageCount += count;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU) - Limit reached event
        if (Limit.HasValue && !wasLimitReached && IsLimitReached() && UserSubscription != null)
        {
            AddDomainEvent(new SubscriptionUsageLimitReachedEvent(
                Id,
                UserSubscriptionId,
                UserSubscription.UserId,
                Feature,
                UsageCount,
                Limit.Value));
        }
    }

    // ✅ BOLUM 1.1: Domain Method - Check if usage limit is reached
    public bool IsLimitReached()
    {
        return Limit.HasValue && UsageCount >= Limit.Value;
    }

    // ✅ BOLUM 1.1: Domain Method - Check if can use feature
    public bool CanUse(int requestedCount = 1)
    {
        Guard.AgainstNegativeOrZero(requestedCount, nameof(requestedCount));

        if (!Limit.HasValue)
            return true; // No limit

        return UsageCount + requestedCount <= Limit.Value;
    }

    // ✅ BOLUM 1.1: Domain Method - Get remaining usage
    public int? GetRemainingUsage()
    {
        if (!Limit.HasValue)
            return null; // Unlimited

        return Math.Max(0, Limit.Value - UsageCount);
    }

    // ✅ BOLUM 1.1: Domain Method - Update limit
    public void UpdateLimit(int? limit)
    {
        if (limit.HasValue)
        {
            Guard.AgainstNegative(limit.Value, nameof(limit));
            
            if (UsageCount > limit.Value)
                throw new DomainException($"Mevcut kullanım ({UsageCount}) yeni limit ({limit.Value})'den fazla olamaz");
        }

        Limit = limit;
        UpdatedAt = DateTime.UtcNow;
    }
}

