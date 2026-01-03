using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Transfer Inventory DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class TransferInventoryDto
{
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Kaynak depo ID zorunludur")]
    public Guid FromWarehouseId { get; set; }

    [Required(ErrorMessage = "Hedef depo ID zorunludur")]
    public Guid ToWarehouseId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Transfer miktarı 1'den büyük olmalıdır.")]
    public int Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
    public string? Notes { get; set; }
}
