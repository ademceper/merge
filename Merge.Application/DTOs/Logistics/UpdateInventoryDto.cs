using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Update Inventory DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class UpdateInventoryDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int MinimumStockLevel { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int MaximumStockLevel { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Birim maliyet 0 veya daha büyük olmalıdır.")]
    public decimal UnitCost { get; set; }
    
    [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
    public string? Location { get; set; }
}
