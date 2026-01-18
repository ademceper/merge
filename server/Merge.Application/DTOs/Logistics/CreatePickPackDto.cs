using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public record CreatePickPackDto(
    [Required]
    Guid OrderId,
    
    [Required]
    Guid WarehouseId,
    
    [StringLength(2000)]
    string? Notes = null
);
