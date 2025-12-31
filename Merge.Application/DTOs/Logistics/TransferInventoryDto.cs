using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Logistics;

public class TransferInventoryDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid FromWarehouseId { get; set; }

    [Required]
    public Guid ToWarehouseId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Transfer miktarı 1'den büyük olmalıdır.")]
    public int Quantity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
