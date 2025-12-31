using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class CreateAccountSecurityEventDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Severity { get; set; } = "Info";
    
    [StringLength(50)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
    
    [StringLength(200)]
    public string? DeviceFingerprint { get; set; }
    
    public bool IsSuspicious { get; set; } = false;
    
    public Dictionary<string, object>? Details { get; set; }
    
    public bool RequiresAction { get; set; } = false;
}
