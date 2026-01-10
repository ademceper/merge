using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Update Inventory DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
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
