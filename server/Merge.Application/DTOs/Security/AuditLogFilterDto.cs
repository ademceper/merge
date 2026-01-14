using Merge.Domain.SharedKernel;
namespace Merge.Application.DTOs.Security;

public class AuditLogFilterDto
{
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? TableName { get; set; }
    public string? Severity { get; set; }
    public string? Module { get; set; }
    public bool? IsSuccessful { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? IpAddress { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}