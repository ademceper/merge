using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Logistics;

public class ReserveStockDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Rezervasyon miktarı 1'den büyük olmalıdır.")]
    public int Quantity { get; set; }

    public Guid? OrderId { get; set; }
}
