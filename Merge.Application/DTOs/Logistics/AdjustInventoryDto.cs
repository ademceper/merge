using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Logistics;

public class AdjustInventoryDto
{
    [Required]
    public Guid InventoryId { get; set; }

    [Required]
    public int QuantityChange { get; set; } // Positive for increase, negative for decrease

    [StringLength(500)]
    public string? Notes { get; set; }
}
