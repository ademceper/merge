using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record CreatePickPackDto(
    [Required]
    Guid OrderId,
    
    [Required]
    Guid WarehouseId,
    
    [StringLength(2000)]
    string? Notes = null
);
