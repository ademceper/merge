using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record UpdateShippingStatusDto(
    [Required]
    [StringLength(50)]
    string Status
);
