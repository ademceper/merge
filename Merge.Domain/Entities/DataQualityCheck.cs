using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// DataQualityCheck Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DataQualityCheck : BaseEntity
{
    public Guid RuleId { get; set; }
    public DataQualityRule Rule { get; set; } = null!;
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public DataQualityCheckStatus Status { get; set; } = DataQualityCheckStatus.Pass;
    public int RecordsChecked { get; set; } = 0;
    public int RecordsPassed { get; set; } = 0;
    public int RecordsFailed { get; set; } = 0;
    public string? ErrorDetails { get; set; } // JSON with failed records
    public DateTime CheckedAt { get; set; }
    public TimeSpan? ExecutionTime { get; set; }
}

