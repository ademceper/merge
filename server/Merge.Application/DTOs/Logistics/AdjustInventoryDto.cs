using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Inventory;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Adjust Inventory DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
public record AdjustInventoryDto(
    [Required(ErrorMessage = "Envanter ID zorunludur")]
    Guid InventoryId,

    [Required(ErrorMessage = "Miktar değişikliği zorunludur")]
    int QuantityChange, // Positive for increase, negative for decrease

    [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
    string? Notes = null
);
