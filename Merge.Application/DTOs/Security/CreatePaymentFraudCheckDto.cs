using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class CreatePaymentFraudCheckDto
{
    [Required]
    public Guid PaymentId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CheckType { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? DeviceFingerprint { get; set; }
    
    [StringLength(50)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
}
