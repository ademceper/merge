using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Analytics;

/// <summary>
/// ReportSchedule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ReportSchedule : BaseAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ReportType Type { get; private set; } = ReportType.Sales;
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;
    public ReportFrequency Frequency { get; private set; } = ReportFrequency.Daily;
    public int DayOfWeek { get; private set; } = 1; // For weekly reports (1=Monday, 7=Sunday)
    public int DayOfMonth { get; private set; } = 1; // For monthly reports
    public TimeSpan TimeOfDay { get; private set; } = TimeSpan.Zero;
    public string? Filters { get; private set; } // JSON object
    public ReportFormat Format { get; private set; } = ReportFormat.PDF;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public string EmailRecipients { get; private set; } = string.Empty; // Comma-separated emails

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private ReportSchedule() { }

    public static ReportSchedule Create(
        string name,
        string description,
        ReportType type,
        Guid ownerId,
        ReportFrequency frequency,
        TimeSpan timeOfDay,
        string? filters = null,
        ReportFormat format = ReportFormat.PDF,
        string emailRecipients = "",
        int dayOfWeek = 1,
        int dayOfMonth = 1)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstDefault(ownerId, nameof(ownerId));

        Guard.AgainstOutOfRange(dayOfWeek, 1, 7, nameof(dayOfWeek));
        Guard.AgainstOutOfRange(dayOfMonth, 1, 31, nameof(dayOfMonth));

        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            OwnerId = ownerId,
            Frequency = frequency,
            DayOfWeek = dayOfWeek,
            DayOfMonth = dayOfMonth,
            TimeOfDay = timeOfDay,
            Filters = filters,
            Format = format,
            EmailRecipients = emailRecipients,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate initial NextRunAt
        schedule.NextRunAt = schedule.CalculateNextRunTime();

        schedule.AddDomainEvent(new ReportScheduleCreatedEvent(
            schedule.Id,
            ownerId,
            type.ToString(),
            frequency.ToString()));

        return schedule;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        NextRunAt = CalculateNextRunTime();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReportScheduleActivatedEvent(
            Id,
            OwnerId,
            NextRunAt));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReportScheduleDeactivatedEvent(
            Id,
            OwnerId));
    }

    public void MarkAsRun()
    {
        LastRunAt = DateTime.UtcNow;
        NextRunAt = CalculateNextRunTime();
        UpdatedAt = DateTime.UtcNow;
    }

    public DateTime CalculateNextRunTime()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.Add(TimeOfDay);

        // If time has passed today, start from tomorrow
        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return Frequency switch
        {
            ReportFrequency.Daily => nextRun,
            ReportFrequency.Weekly => CalculateNextWeeklyRun(nextRun),
            ReportFrequency.Monthly => CalculateNextMonthlyRun(nextRun),
            ReportFrequency.Quarterly => CalculateNextQuarterlyRun(nextRun),
            ReportFrequency.Yearly => CalculateNextYearlyRun(nextRun),
            _ => nextRun
        };
    }

    private DateTime CalculateNextWeeklyRun(DateTime baseDate)
    {
        var targetDayOfWeek = (DayOfWeek)(DayOfWeek - 1); // Convert 1-7 to DayOfWeek enum
        var daysUntilTarget = ((int)targetDayOfWeek - (int)baseDate.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0 && baseDate.Date == DateTime.UtcNow.Date && baseDate.TimeOfDay <= TimeOfDay)
            daysUntilTarget = 7;
        return baseDate.AddDays(daysUntilTarget);
    }

    private DateTime CalculateNextMonthlyRun(DateTime baseDate)
    {
        var nextMonth = baseDate.AddMonths(1);
        var dayOfMonth = Math.Min(DayOfMonth, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        return new DateTime(nextMonth.Year, nextMonth.Month, dayOfMonth, TimeOfDay.Hours, TimeOfDay.Minutes, TimeOfDay.Seconds);
    }

    private DateTime CalculateNextQuarterlyRun(DateTime baseDate)
    {
        var nextQuarter = baseDate.AddMonths(3);
        var dayOfMonth = Math.Min(DayOfMonth, DateTime.DaysInMonth(nextQuarter.Year, nextQuarter.Month));
        return new DateTime(nextQuarter.Year, nextQuarter.Month, dayOfMonth, TimeOfDay.Hours, TimeOfDay.Minutes, TimeOfDay.Seconds);
    }

    private DateTime CalculateNextYearlyRun(DateTime baseDate)
    {
        var nextYear = baseDate.AddYears(1);
        var dayOfMonth = Math.Min(DayOfMonth, DateTime.DaysInMonth(nextYear.Year, nextYear.Month));
        return new DateTime(nextYear.Year, nextYear.Month, dayOfMonth, TimeOfDay.Hours, TimeOfDay.Minutes, TimeOfDay.Seconds);
    }

    public bool IsDue()
    {
        return IsActive && NextRunAt.HasValue && NextRunAt.Value <= DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReportScheduleDeletedEvent(
            Id,
            OwnerId));
    }
}

