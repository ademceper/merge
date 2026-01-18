using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.B2B;


public class RejectPODto
{
    [Required(ErrorMessage = "Red sebebi zorunludur")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Red sebebi en az 5, en fazla 1000 karakter olmalıdır")]
    public string Reason { get; set; } = string.Empty;
}
