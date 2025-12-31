using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class UpdateOrderVerificationDto
{
    [StringLength(50)]
    public string? Status { get; set; }
    
    [StringLength(2000)]
    public string? VerificationNotes { get; set; }
    
    [StringLength(1000)]
    public string? RejectionReason { get; set; }
}
