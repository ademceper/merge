using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class UpdateTrackingDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Takip numarası gereklidir.")]
    public string TrackingNumber { get; set; } = string.Empty;
}
