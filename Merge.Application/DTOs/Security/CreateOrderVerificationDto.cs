using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class CreateOrderVerificationDto
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string VerificationType { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? VerificationMethod { get; set; }
    
    [StringLength(2000)]
    public string? VerificationNotes { get; set; }
    
    public bool RequiresManualReview { get; set; } = false;
}
