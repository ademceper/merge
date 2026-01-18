using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record CreateSizeGuideDto(
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Beden kılavuzu adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(2000)]
    string Description,
    
    [Required]
    Guid CategoryId,
    
    [Required]
    [MinLength(1, ErrorMessage = "En az bir beden girişi gereklidir.")]
    IReadOnlyList<CreateSizeGuideEntryDto> Entries,
    
    [StringLength(100)]
    string? Brand = null,
    
    [StringLength(50)]
    string Type = "Standard",
    
    [StringLength(20)]
    string MeasurementUnit = "cm"
);
