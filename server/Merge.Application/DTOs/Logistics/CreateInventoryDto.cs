using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Inventory;

namespace Merge.Application.DTOs.Logistics;


public record CreateInventoryDto(
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    Guid ProductId,

    [Required(ErrorMessage = "Depo ID zorunludur")]
    Guid WarehouseId,

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(0, int.MaxValue, ErrorMessage = "Miktar 0 veya daha büyük olmalıdır.")]
    int Quantity,

    [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    int MinimumStockLevel = 0,

    [Range(0, int.MaxValue, ErrorMessage = "Maksimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    int MaximumStockLevel = 0,

    [Range(0, double.MaxValue, ErrorMessage = "Birim maliyet 0 veya daha büyük olmalıdır.")]
    decimal UnitCost = 0,

    [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
    string? Location = null
);
