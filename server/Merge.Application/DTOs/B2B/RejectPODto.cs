using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.B2B;

/// <summary>
/// Purchase Order rejection DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class RejectPODto
{
    [Required(ErrorMessage = "Red sebebi zorunludur")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Red sebebi en az 5, en fazla 1000 karakter olmalıdır")]
    public string Reason { get; set; } = string.Empty;
}
