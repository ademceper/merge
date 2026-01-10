using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Reserve Stock DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
public record ReserveStockDto(
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    Guid ProductId,

    [Required(ErrorMessage = "Depo ID zorunludur")]
    Guid WarehouseId,

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Rezervasyon miktarı 1'den büyük olmalıdır.")]
    int Quantity,

    Guid? OrderId = null
);
