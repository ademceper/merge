using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// SubscriptionUsage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionUsage : BaseEntity, IAggregateRoot
{
    public Guid UserSubscriptionId { get; private set; }
    public string Feature { get; private set; } = string.Empty; // Feature name being used
    public int UsageCount { get; private set; } = 0;
    public int? Limit { get; private set; } // Usage limit for this feature
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public UserSubscription UserSubscription { get; private set; } = null!;

    private SubscriptionUsage() { }

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
            throw new DomainException("Limit cannot be negative");

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

        usage.AddDomainEvent(new SubscriptionUsageCreatedEvent(
            usage.Id,
            subscription.Id,
            subscription.UserId,
            feature,
            periodStart,
            periodEnd,
            limit));

        return usage;
    }

    public void IncrementUsage(int count = 1)
    {
        Guard.AgainstNegativeOrZero(count, nameof(count));

        if (Limit.HasValue && UsageCount + count > Limit.Value)
            throw new DomainException($"Kullanım limiti aşıldı. Limit: {Limit.Value}, Mevcut: {UsageCount}, İstenen: {count}");

        var wasLimitReached = IsLimitReached();
        UsageCount += count;
        UpdatedAt = DateTime.UtcNow;

        if (Limit.HasValue && !wasLimitReached && IsLimitReached() && UserSubscription is not null)
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

    public bool IsLimitReached()
    {
        return Limit.HasValue && UsageCount >= Limit.Value;
    }

    public bool CanUse(int requestedCount = 1)
    {
        Guard.AgainstNegativeOrZero(requestedCount, nameof(requestedCount));

        if (!Limit.HasValue)
            return true; // No limit

        return UsageCount + requestedCount <= Limit.Value;
    }

    public int? GetRemainingUsage()
    {
        if (!Limit.HasValue)
            return null; // Unlimited

        return Math.Max(0, Limit.Value - UsageCount);
    }

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

        AddDomainEvent(new SubscriptionUsageUpdatedEvent(
            Id,
            UserSubscriptionId,
            UserSubscription?.UserId ?? Guid.Empty,
            Feature,
            UsageCount,
            limit));
    }
}

