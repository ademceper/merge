using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Logistics;

public class CreateInventoryDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Miktar 0 veya daha büyük olmalıdır.")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumStockLevel { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int MaximumStockLevel { get; set; } = 0;

    [Range(0, double.MaxValue)]
    public decimal UnitCost { get; set; } = 0;

    [StringLength(200)]
    public string? Location { get; set; }
}
