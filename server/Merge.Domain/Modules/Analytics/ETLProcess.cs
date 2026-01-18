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
public class ETLProcess(
    string name,
    string description,
    string processType,
    TransactionStatus status,
    string? sourceSystem,
    string? targetSystem,
    string? schedule,
    string? configuration) : BaseAggregateRoot
{
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;
    public string ProcessType { get; private set; } = processType; // Extract, Transform, Load
    public TransactionStatus Status { get; private set; } = status;
    public string? SourceSystem { get; private set; } = sourceSystem;
    public string? TargetSystem { get; private set; } = targetSystem;
    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public string? Schedule { get; private set; } = schedule; // Cron expression or schedule description
    public int RecordsProcessed { get; private set; } = 0;
    public int RecordsFailed { get; private set; } = 0;
    public string? ErrorMessage { get; private set; }
    public TimeSpan? ExecutionTime { get; private set; }
    public string? Configuration { get; private set; } = configuration; // JSON configuration

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private ETLProcess() : this(string.Empty, string.Empty, string.Empty, TransactionStatus.Pending, null, null, null, null) { }

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

        var process = new ETLProcess(
            name,
            description ?? string.Empty,
            processType,
            TransactionStatus.Pending,
            sourceSystem,
            targetSystem,
            schedule,
            configuration)
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        process.AddDomainEvent(new ETLProcessCreatedEvent(
            process.Id,
            name,
            processType,
            sourceSystem ?? string.Empty,
            targetSystem ?? string.Empty));

        return process;
    }

    public void Start()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException($"ETL process {Status} durumunda, başlatılamaz");

        Status = TransactionStatus.Running;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ETLProcessStartedEvent(Id));
    }

    public void Complete(int recordsProcessed, TimeSpan executionTime)
    {
        if (Status != TransactionStatus.Running)
            throw new InvalidOperationException($"ETL process {Status} durumunda, tamamlanamaz");

        if (recordsProcessed < 0)
            throw new DomainException("İşlenen kayıt sayısı negatif olamaz");

        Status = TransactionStatus.Completed;
        LastRunAt = DateTime.UtcNow;
        RecordsProcessed = recordsProcessed;
        ExecutionTime = executionTime;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ETLProcessCompletedEvent(
            Id,
            recordsProcessed,
            executionTime));
    }

    public void Fail(int recordsProcessed, int recordsFailed, string errorMessage, TimeSpan? executionTime = null)
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidOperationException("Tamamlanmış ETL process başarısız olarak işaretlenemez");

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

        AddDomainEvent(new ETLProcessFailedEvent(
            Id,
            recordsProcessed,
            recordsFailed,
            errorMessage));
    }

    public void ScheduleNextRun(DateTime nextRunAt)
    {
        if (nextRunAt <= DateTime.UtcNow)
            throw new DomainException("Sonraki çalıştırma tarihi gelecekte olmalıdır");

        NextRunAt = nextRunAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConfiguration(string configuration)
    {
        Configuration = configuration;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsDue()
    {
        return Status == TransactionStatus.Pending && 
               NextRunAt.HasValue && 
               NextRunAt.Value <= DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ETLProcessDeletedEvent(Id));
    }
}

