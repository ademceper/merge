using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Order;

public class CompleteReturnDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Takip numarası gereklidir.")]
    public string TrackingNumber { get; set; } = string.Empty;
}
