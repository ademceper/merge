namespace Merge.Application.DTOs.Security;

public class TopAuditUserDto
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int ActionCount { get; set; }
    public DateTime LastAction { get; set; }
}
