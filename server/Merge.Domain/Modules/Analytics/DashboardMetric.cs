using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;

namespace Merge.Domain.Modules.Analytics;

/// <summary>
/// DashboardMetric Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DashboardMetric : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty; // Unique identifier like 'total_revenue', 'total_orders'
    public string Category { get; private set; } = string.Empty; // Sales, Customers, Products, Inventory
    public decimal Value { get; private set; } = 0;
    public string? ValueFormatted { get; private set; }
    public decimal? PreviousValue { get; private set; }
    public decimal? ChangePercentage { get; private set; }
    public DateTime CalculatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public string? Metadata { get; private set; } // JSON for additional data

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private DashboardMetric() { }

    public static DashboardMetric Create(
        string key,
        string name,
        string category,
        decimal value,
        DateTime periodStart,
        DateTime periodEnd,
        decimal? previousValue = null,
        string? valueFormatted = null,
        string? metadata = null)
    {
        Guard.AgainstNullOrEmpty(key, nameof(key));
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(category, nameof(category));

        if (periodStart >= periodEnd)
            throw new DomainException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        var metric = new DashboardMetric
        {
            Id = Guid.NewGuid(),
            Key = key,
            Name = name,
            Category = category,
            Value = value,
            PreviousValue = previousValue,
            ValueFormatted = valueFormatted,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Metadata = metadata,
            CalculatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate change percentage if previous value exists
        if (previousValue.HasValue)
        {
            metric.ChangePercentage = metric.CalculateChangePercentage(previousValue.Value);
        }

        return metric;
    }

    public void UpdateValue(decimal newValue, decimal? previousValue = null)
    {
        PreviousValue = Value;
        Value = newValue;
        
        if (previousValue.HasValue)
        {
            ChangePercentage = CalculateChangePercentage(previousValue.Value);
        }
        else if (PreviousValue.HasValue)
        {
            ChangePercentage = CalculateChangePercentage(PreviousValue.Value);
        }

        CalculatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal? CalculateChangePercentage(decimal previousValue)
    {
        if (previousValue == 0)
            return Value != 0 ? 100 : 0;

        return ((Value - previousValue) / previousValue) * 100;
    }

    public void SetFormattedValue(string formattedValue)
    {
        ValueFormatted = formattedValue;
        UpdatedAt = DateTime.UtcNow;
    }
}

