using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Order;

public class UpdateOrderStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}

