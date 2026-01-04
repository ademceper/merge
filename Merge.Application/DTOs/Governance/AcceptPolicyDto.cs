using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Governance;

/// <summary>
/// Policy kabul DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class AcceptPolicyDto
{
    [Required(ErrorMessage = "Policy ID zorunludur")]
    public Guid PolicyId { get; set; }
}
