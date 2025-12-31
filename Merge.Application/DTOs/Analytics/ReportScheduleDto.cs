namespace Merge.Application.DTOs.Analytics;

public class ReportScheduleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string EmailRecipients { get; set; } = string.Empty;
}
