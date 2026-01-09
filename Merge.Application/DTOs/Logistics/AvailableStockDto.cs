namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Available Stock DTO - BOLUM 4.3: Over-Posting Koruması (Anonymous object YASAK)
/// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
public record AvailableStockDto(
    Guid ProductId,
    Guid? WarehouseId,
    int AvailableStock
);

