using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// ETLProcess Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ETLProcess : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProcessType { get; set; } = string.Empty; // Extract, Transform, Load
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? SourceSystem { get; set; }
    public string? TargetSystem { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string? Schedule { get; set; } // Cron expression or schedule description
    public int RecordsProcessed { get; set; } = 0;
    public int RecordsFailed { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public TimeSpan? ExecutionTime { get; set; }
    public string? Configuration { get; set; } // JSON configuration
}

