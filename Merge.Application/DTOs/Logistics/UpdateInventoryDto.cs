using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class UpdateInventoryDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int MinimumStockLevel { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int MaximumStockLevel { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Birim maliyet 0 veya daha büyük olmalıdır.")]
    public decimal UnitCost { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
}
