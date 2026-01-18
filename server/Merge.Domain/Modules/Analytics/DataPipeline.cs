using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Analytics;

/// <summary>
/// DataPipeline Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot gerekli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DataPipeline : BaseAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public EntityStatus Status { get; private set; } = EntityStatus.Inactive;
    public string? SourceType { get; private set; } // Database, API, File, Stream
    public string? TargetType { get; private set; } // Database, DataWarehouse, API, File
    public string? SourceConfig { get; private set; } // JSON configuration
    public string? TargetConfig { get; private set; } // JSON configuration
    public string? TransformationRules { get; private set; } // JSON transformation rules
    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public string? Schedule { get; private set; }
    public int RecordsProcessed { get; private set; } = 0;
    public int RecordsFailed { get; private set; } = 0;
    public string? ErrorMessage { get; private set; }
    public bool IsRealTime { get; private set; } = false;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private DataPipeline() { }

    public static DataPipeline Create(
        string name,
        string description,
        string? sourceType = null,
        string? targetType = null,
        string? sourceConfig = null,
        string? targetConfig = null,
        string? transformationRules = null,
        string? schedule = null,
        bool isRealTime = false)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));

        var pipeline = new DataPipeline
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            Status = EntityStatus.Inactive,
            SourceType = sourceType,
            TargetType = targetType,
            SourceConfig = sourceConfig,
            TargetConfig = targetConfig,
            TransformationRules = transformationRules,
            Schedule = schedule,
            IsRealTime = isRealTime,
            RecordsProcessed = 0,
            RecordsFailed = 0,
            CreatedAt = DateTime.UtcNow
        };

        pipeline.AddDomainEvent(new DataPipelineCreatedEvent(
            pipeline.Id,
            name,
            sourceType ?? string.Empty,
            targetType ?? string.Empty));

        return pipeline;
    }

    public void Activate()
    {
        if (Status == EntityStatus.Active)
            return;

        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataPipelineActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.Inactive)
            return;

        Status = EntityStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataPipelineDeactivatedEvent(Id));
    }

    public void UpdateConfiguration(string? sourceConfig, string? targetConfig, string? transformationRules)
    {
        SourceConfig = sourceConfig;
        TargetConfig = targetConfig;
        TransformationRules = transformationRules;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRun(int recordsProcessed, int recordsFailed = 0, string? errorMessage = null)
    {
        if (recordsProcessed < 0)
            throw new DomainException("İşlenen kayıt sayısı negatif olamaz");
        if (recordsFailed < 0)
            throw new DomainException("Başarısız kayıt sayısı negatif olamaz");

        LastRunAt = DateTime.UtcNow;
        RecordsProcessed = recordsProcessed;
        RecordsFailed = recordsFailed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataPipelineRunCompletedEvent(
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

    public bool IsDue()
    {
        return Status == EntityStatus.Active && 
               NextRunAt.HasValue && 
               NextRunAt.Value <= DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataPipelineDeletedEvent(Id));
    }
}

