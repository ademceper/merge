namespace Merge.Application.DTOs.Analytics;

public record ReportScheduleDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string Frequency,
    string Format,
    bool IsActive,
    DateTime? LastRunAt,
    DateTime? NextRunAt,
    string EmailRecipients
);
