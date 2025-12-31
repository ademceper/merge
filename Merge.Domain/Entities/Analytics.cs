namespace Merge.Domain.Entities;

public class Report : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; } = ReportType.Sales;
    public Guid GeneratedBy { get; set; }
    public User GeneratedByUser { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Filters { get; set; } // JSON object of applied filters
    public string? Data { get; set; } // JSON serialized report data
    public string? FilePath { get; set; } // Path to exported file if applicable
    public ReportFormat Format { get; set; } = ReportFormat.JSON;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ReportSchedule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; } = ReportType.Sales;
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public ReportFrequency Frequency { get; set; } = ReportFrequency.Daily;
    public int DayOfWeek { get; set; } = 1; // For weekly reports (1=Monday, 7=Sunday)
    public int DayOfMonth { get; set; } = 1; // For monthly reports
    public TimeSpan TimeOfDay { get; set; } = TimeSpan.Zero;
    public string? Filters { get; set; } // JSON object
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string EmailRecipients { get; set; } = string.Empty; // Comma-separated emails
}

public class DashboardMetric : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty; // Unique identifier like 'total_revenue', 'total_orders'
    public string Category { get; set; } = string.Empty; // Sales, Customers, Products, Inventory
    public decimal Value { get; set; } = 0;
    public string? ValueFormatted { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? ChangePercentage { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? Metadata { get; set; } // JSON for additional data
}

public enum ReportType
{
    Sales,
    Revenue,
    Products,
    Inventory,
    Customers,
    Orders,
    Marketing,
    Financial,
    Tax,
    Shipping,
    Returns,
    Custom
}

public enum ReportFormat
{
    JSON,
    CSV,
    Excel,
    PDF
}

public enum ReportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public enum ReportFrequency
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}
