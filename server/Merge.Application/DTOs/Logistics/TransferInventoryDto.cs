using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Inventory;

namespace Merge.Application.DTOs.Logistics;


public record TransferInventoryDto(
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    Guid ProductId,

    [Required(ErrorMessage = "Kaynak depo ID zorunludur")]
    Guid FromWarehouseId,

    [Required(ErrorMessage = "Hedef depo ID zorunludur")]
    Guid ToWarehouseId,

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Transfer miktarı 1'den büyük olmalıdır.")]
    int Quantity,

    [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
    string? Notes = null
);
