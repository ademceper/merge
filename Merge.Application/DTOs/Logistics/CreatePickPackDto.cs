using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class CreatePickPackDto
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public Guid WarehouseId { get; set; }
    
    [StringLength(2000)]
    public string? Notes { get; set; }
}
