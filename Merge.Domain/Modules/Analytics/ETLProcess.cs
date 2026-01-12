using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Analytics;

/// <summary>
/// ETLProcess Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot gerekli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ETLProcess : BaseAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ProcessType { get; private set; } = string.Empty; // Extract, Transform, Load
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public TransactionStatus Status { get; private set; } = TransactionStatus.Pending;
    public string? SourceSystem { get; private set; }
    public string? TargetSystem { get; private set; }
    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public string? Schedule { get; private set; } // Cron expression or schedule description
    public int RecordsProcessed { get; private set; } = 0;
    public int RecordsFailed { get; private set; } = 0;
    public string? ErrorMessage { get; private set; }
    public TimeSpan? ExecutionTime { get; private set; }
    public string? Configuration { get; private set; } // JSON configuration

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ETLProcess() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static ETLProcess Create(
        string name,
        string description,
        string processType,
        string? sourceSystem = null,
        string? targetSystem = null,
        string? schedule = null,
        string? configuration = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(processType, nameof(processType));

        var process = new ETLProcess
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            ProcessType = processType,
            Status = TransactionStatus.Pending,
            SourceSystem = sourceSystem,
            TargetSystem = targetSystem,
            Schedule = schedule,
            Configuration = configuration,
            RecordsProcessed = 0,
            RecordsFailed = 0,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - ETLProcessCreatedEvent yayınla
        process.AddDomainEvent(new ETLProcessCreatedEvent(
            process.Id,
            name,
            processType,
            sourceSystem ?? string.Empty,
            targetSystem ?? string.Empty));

        return process;
    }

    // ✅ BOLUM 1.1: Domain Logic - Start process
    public void Start()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException($"ETL process {Status} durumunda, başlatılamaz");

        Status = TransactionStatus.Running;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ETLProcessStartedEvent yayınla
        AddDomainEvent(new ETLProcessStartedEvent(Id));
    }

    // ✅ BOLUM 1.1: Domain Logic - Complete process
    public void Complete(int recordsProcessed, TimeSpan executionTime)
    {
        if (Status != TransactionStatus.Running)
            throw new InvalidOperationException($"ETL process {Status} durumunda, tamamlanamaz");

        // ✅ BOLUM 1.6: Invariant Validation - RecordsProcessed >= 0
        if (recordsProcessed < 0)
            throw new DomainException("İşlenen kayıt sayısı negatif olamaz");

        Status = TransactionStatus.Completed;
        LastRunAt = DateTime.UtcNow;
        RecordsProcessed = recordsProcessed;
        ExecutionTime = executionTime;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ETLProcessCompletedEvent yayınla
        AddDomainEvent(new ETLProcessCompletedEvent(
            Id,
            recordsProcessed,
            executionTime));
    }

    // ✅ BOLUM 1.1: Domain Logic - Fail process
    public void Fail(int recordsProcessed, int recordsFailed, string errorMessage, TimeSpan? executionTime = null)
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidOperationException("Tamamlanmış ETL process başarısız olarak işaretlenemez");

        // ✅ BOLUM 1.6: Invariant Validation - RecordsProcessed >= 0, RecordsFailed >= 0
        if (recordsProcessed < 0)
            throw new DomainException("İşlenen kayıt sayısı negatif olamaz");
        if (recordsFailed < 0)
            throw new DomainException("Başarısız kayıt sayısı negatif olamaz");
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));

        Status = TransactionStatus.Failed;
        LastRunAt = DateTime.UtcNow;
        RecordsProcessed = recordsProcessed;
        RecordsFailed = recordsFailed;
        ErrorMessage = errorMessage;
        ExecutionTime = executionTime;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ETLProcessFailedEvent yayınla
        AddDomainEvent(new ETLProcessFailedEvent(
            Id,
            recordsProcessed,
            recordsFailed,
            errorMessage));
    }

    // ✅ BOLUM 1.1: Domain Logic - Schedule next run
    public void ScheduleNextRun(DateTime nextRunAt)
    {
        if (nextRunAt <= DateTime.UtcNow)
            throw new DomainException("Sonraki çalıştırma tarihi gelecekte olmalıdır");

        NextRunAt = nextRunAt;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update configuration
    public void UpdateConfiguration(string configuration)
    {
        Configuration = configuration;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if process is due
    public bool IsDue()
    {
        return Status == TransactionStatus.Pending && 
               NextRunAt.HasValue && 
               NextRunAt.Value <= DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ETLProcessDeletedEvent yayınla
        AddDomainEvent(new ETLProcessDeletedEvent(Id));
    }
}

