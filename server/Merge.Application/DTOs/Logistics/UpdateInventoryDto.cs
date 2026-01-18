using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Inventory;

namespace Merge.Application.DTOs.Logistics;


public record UpdateInventoryDto(
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    int? MinimumStockLevel = null,
    
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum stok seviyesi 0 veya daha büyük olmalıdır.")]
    int? MaximumStockLevel = null,
    
    [Range(0, double.MaxValue, ErrorMessage = "Birim maliyet 0 veya daha büyük olmalıdır.")]
    decimal? UnitCost = null,
    
    [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
    string? Location = null
);
