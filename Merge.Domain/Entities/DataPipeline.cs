using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// DataPipeline Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DataPipeline : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public EntityStatus Status { get; set; } = EntityStatus.Inactive;
    public string? SourceType { get; set; } // Database, API, File, Stream
    public string? TargetType { get; set; } // Database, DataWarehouse, API, File
    public string? SourceConfig { get; set; } // JSON configuration
    public string? TargetConfig { get; set; } // JSON configuration
    public string? TransformationRules { get; set; } // JSON transformation rules
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string? Schedule { get; set; }
    public int RecordsProcessed { get; set; } = 0;
    public int RecordsFailed { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public bool IsRealTime { get; set; } = false;
}

