using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Release Stock DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class ReleaseStockDto
{
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Depo ID zorunludur")]
    public Guid WarehouseId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Serbest bırakma miktarı 1'den büyük olmalıdır.")]
    public int Quantity { get; set; }

    public Guid? OrderId { get; set; }
}
