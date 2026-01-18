using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Analytics;

/// <summary>
/// DataQualityCheck Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot gerekli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DataQualityCheck : BaseAggregateRoot
{
    public Guid RuleId { get; private set; }
    public DataQualityRule Rule { get; private set; } = null!;
    public DataQualityCheckStatus Status { get; private set; } = DataQualityCheckStatus.Pass;
    public int RecordsChecked { get; private set; } = 0;
    public int RecordsPassed { get; private set; } = 0;
    public int RecordsFailed { get; private set; } = 0;
    public string? ErrorDetails { get; private set; } // JSON with failed records
    public DateTime CheckedAt { get; private set; }
    public TimeSpan? ExecutionTime { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private DataQualityCheck() { }

    public static DataQualityCheck Create(
        Guid ruleId,
        int recordsChecked,
        int recordsPassed,
        int recordsFailed,
        TimeSpan? executionTime = null,
        string? errorDetails = null)
    {
        Guard.AgainstDefault(ruleId, nameof(ruleId));

        if (recordsChecked < 0)
            throw new DomainException("Kontrol edilen kayıt sayısı negatif olamaz");
        if (recordsPassed < 0)
            throw new DomainException("Geçen kayıt sayısı negatif olamaz");
        if (recordsFailed < 0)
            throw new DomainException("Başarısız kayıt sayısı negatif olamaz");
        if (recordsPassed + recordsFailed > recordsChecked)
            throw new DomainException("Geçen ve başarısız kayıt sayılarının toplamı kontrol edilen kayıt sayısından fazla olamaz");

        var status = recordsFailed == 0 ? DataQualityCheckStatus.Pass : DataQualityCheckStatus.Fail;

        var check = new DataQualityCheck
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            Status = status,
            RecordsChecked = recordsChecked,
            RecordsPassed = recordsPassed,
            RecordsFailed = recordsFailed,
            ErrorDetails = errorDetails,
            CheckedAt = DateTime.UtcNow,
            ExecutionTime = executionTime,
            CreatedAt = DateTime.UtcNow
        };

        check.AddDomainEvent(new DataQualityCheckCreatedEvent(
            check.Id,
            ruleId,
            recordsChecked,
            recordsPassed,
            recordsFailed,
            status));

        return check;
    }

    public void UpdateResults(int recordsChecked, int recordsPassed, int recordsFailed, string? errorDetails = null)
    {
        if (recordsChecked < 0)
            throw new DomainException("Kontrol edilen kayıt sayısı negatif olamaz");
        if (recordsPassed < 0)
            throw new DomainException("Geçen kayıt sayısı negatif olamaz");
        if (recordsFailed < 0)
            throw new DomainException("Başarısız kayıt sayısı negatif olamaz");
        if (recordsPassed + recordsFailed > recordsChecked)
            throw new DomainException("Geçen ve başarısız kayıt sayılarının toplamı kontrol edilen kayıt sayısından fazla olamaz");

        RecordsChecked = recordsChecked;
        RecordsPassed = recordsPassed;
        RecordsFailed = recordsFailed;
        ErrorDetails = errorDetails;
        Status = recordsFailed == 0 ? DataQualityCheckStatus.Pass : DataQualityCheckStatus.Fail;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataQualityCheckDeletedEvent(
            Id,
            RuleId));
    }
}

