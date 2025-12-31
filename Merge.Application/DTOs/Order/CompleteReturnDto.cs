using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Order;

public class CompleteReturnDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Takip numarası gereklidir.")]
    public string TrackingNumber { get; set; } = string.Empty;
}
