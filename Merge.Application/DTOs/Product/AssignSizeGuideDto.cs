using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record AssignSizeGuideDto(
    [Required] Guid ProductId,
    [Required] Guid SizeGuideId,
    [StringLength(1000)] string? CustomNotes,
    bool FitType,
    [StringLength(2000)] string? FitDescription
);
