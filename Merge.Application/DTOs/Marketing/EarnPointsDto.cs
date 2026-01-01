using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class EarnPointsDto
{
    [Required(ErrorMessage = "Points value is required")]
    [Range(1, 1000000, ErrorMessage = "Points must be between 1 and 1000000")]
    public int Points { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 500 characters")]
    public string Description { get; set; } = string.Empty;
}
