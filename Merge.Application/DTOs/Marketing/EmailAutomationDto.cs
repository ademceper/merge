namespace Merge.Application.DTOs.Marketing;

public class EmailAutomationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int DelayHours { get; set; }
    public int TotalTriggered { get; set; }
    public int TotalSent { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
}
