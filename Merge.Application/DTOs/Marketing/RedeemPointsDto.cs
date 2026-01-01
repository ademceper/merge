using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class RedeemPointsDto
{
    [Required(ErrorMessage = "Points value is required")]
    [Range(1, 1000000, ErrorMessage = "Points must be between 1 and 1000000")]
    public int Points { get; set; }

    public Guid? OrderId { get; set; }
}
