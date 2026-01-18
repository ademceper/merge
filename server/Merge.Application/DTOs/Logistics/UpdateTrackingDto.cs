using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public record UpdateTrackingDto(
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Takip numarası gereklidir.")]
    string TrackingNumber
);
