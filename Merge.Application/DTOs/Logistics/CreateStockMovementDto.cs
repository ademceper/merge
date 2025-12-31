using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;

namespace Merge.Application.DTOs.Logistics;

public class CreateStockMovementDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid WarehouseId { get; set; }
    
    [Required]
    public StockMovementType MovementType { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    public int Quantity { get; set; }
    
    [StringLength(100)]
    public string? ReferenceNumber { get; set; }
    
    public Guid? ReferenceId { get; set; }
    
    [StringLength(2000)]
    public string? Notes { get; set; }
    
    public Guid? FromWarehouseId { get; set; }
    
    public Guid? ToWarehouseId { get; set; }
}
