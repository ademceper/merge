namespace Merge.Application.DTOs.Security;

public class EntityAuditHistoryDto
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public List<AuditLogDto> AuditLogs { get; set; } = new();
    public DateTime FirstChange { get; set; }
    public DateTime LastChange { get; set; }
    public int TotalChanges { get; set; }
    public List<string> ModifiedBy { get; set; } = new();
}
