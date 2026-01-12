using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.User;

public class ActivityFilterDto
{
    public Guid? UserId { get; set; }
    public string? ActivityType { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceType { get; set; }
    public bool? WasSuccessful { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
