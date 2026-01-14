using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record UpdateTrackingDto(
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Takip numarası gereklidir.")]
    string TrackingNumber
);
