using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Create Inventory DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class CreateInventoryDto
{
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Depo ID zorunludur")]
    public Guid WarehouseId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(0, int.MaxValue, ErrorMessage = "Miktar 0 veya daha büyük olmalıdır.")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int MinimumStockLevel { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Maksimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    public int MaximumStockLevel { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "Birim maliyet 0 veya daha büyük olmalıdır.")]
    public decimal UnitCost { get; set; } = 0;

    [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
    public string? Location { get; set; }
}
