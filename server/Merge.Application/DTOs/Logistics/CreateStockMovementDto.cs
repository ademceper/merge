using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Logistics;

public record CreateStockMovementDto(
    [Required]
    Guid ProductId,
    
    [Required]
    Guid WarehouseId,
    
    [Required]
    StockMovementType MovementType,
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    int Quantity,
    
    [StringLength(100)]
    string? ReferenceNumber = null,
    
    Guid? ReferenceId = null,
    
    [StringLength(2000)]
    string? Notes = null,
    
    Guid? FromWarehouseId = null,
    
    Guid? ToWarehouseId = null
);
