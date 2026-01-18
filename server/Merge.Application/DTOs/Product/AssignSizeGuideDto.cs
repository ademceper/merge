using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record AssignSizeGuideDto(
    [Required] Guid ProductId,
    [Required] Guid SizeGuideId,
    [StringLength(1000)] string? CustomNotes,
    bool FitType,
    [StringLength(2000)] string? FitDescription
);
