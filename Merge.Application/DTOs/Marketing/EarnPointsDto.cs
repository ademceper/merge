using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Earn Points DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record EarnPointsDto
{
    [Required(ErrorMessage = "Points value is required")]
    [Range(1, 1000000, ErrorMessage = "Points must be between 1 and 1000000")]
    public int Points { get; init; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 500 characters")]
    public string Description { get; init; } = string.Empty;
}
