using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class UpdateShippingStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}
