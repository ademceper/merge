using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Adjust Inventory DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class AdjustInventoryDto
{
    [Required(ErrorMessage = "Envanter ID zorunludur")]
    public Guid InventoryId { get; set; }

    [Required(ErrorMessage = "Miktar değişikliği zorunludur")]
    public int QuantityChange { get; set; } // Positive for increase, negative for decrease

    [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
    public string? Notes { get; set; }
}
