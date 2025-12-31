namespace Merge.Domain.Entities;

public class DataWarehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty; // Source system identifier
    public string Status { get; set; } = "Active"; // Active, Inactive, Maintenance
    public DateTime? LastSyncAt { get; set; }
    public DateTime? NextSyncAt { get; set; }
    public string? SyncFrequency { get; set; } // Daily, Weekly, Monthly, Real-time
    public int RecordCount { get; set; } = 0;
    public long DataSize { get; set; } = 0; // In bytes
    public string? Schema { get; set; } // JSON schema definition
    public string? Metadata { get; set; } // JSON metadata
}

public class ETLProcess : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProcessType { get; set; } = string.Empty; // Extract, Transform, Load
    public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed
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

public class DataPipeline : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Inactive"; // Active, Inactive, Paused, Error
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

public class DataQualityRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // Completeness, Accuracy, Consistency, Validity, Timeliness
    public string TargetEntity { get; set; } = string.Empty; // Entity/Table name
    public string? FieldName { get; set; } // Specific field to check
    public string? RuleExpression { get; set; } // SQL or expression for validation
    public string Status { get; set; } = "Active"; // Active, Inactive
    public int PassCount { get; set; } = 0;
    public int FailCount { get; set; } = 0;
    public decimal PassRate { get; set; } = 100; // Percentage
    public DateTime? LastCheckAt { get; set; }
    public string? Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
}

public class DataQualityCheck : BaseEntity
{
    public Guid RuleId { get; set; }
    public DataQualityRule Rule { get; set; } = null!;
    public string Status { get; set; } = string.Empty; // Pass, Fail, Warning
    public int RecordsChecked { get; set; } = 0;
    public int RecordsPassed { get; set; } = 0;
    public int RecordsFailed { get; set; } = 0;
    public string? ErrorDetails { get; set; } // JSON with failed records
    public DateTime CheckedAt { get; set; }
    public TimeSpan? ExecutionTime { get; set; }
}

