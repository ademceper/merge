using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;


public record RedeemPointsDto
{
    [Required(ErrorMessage = "Points value is required")]
    [Range(1, 1000000, ErrorMessage = "Points must be between 1 and 1000000")]
    public int Points { get; init; }

    public Guid? OrderId { get; init; }
}
