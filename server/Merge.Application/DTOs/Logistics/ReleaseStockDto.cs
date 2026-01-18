using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;


public record ReleaseStockDto(
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    Guid ProductId,

    [Required(ErrorMessage = "Depo ID zorunludur")]
    Guid WarehouseId,

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Serbest bırakma miktarı 1'den büyük olmalıdır.")]
    int Quantity,

    Guid? OrderId = null
);
