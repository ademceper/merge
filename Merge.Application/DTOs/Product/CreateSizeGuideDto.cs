using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class CreateSizeGuideDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Beden kılavuzu adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Guid CategoryId { get; set; }
    
    [StringLength(100)]
    public string? Brand { get; set; }
    
    [StringLength(50)]
    public string Type { get; set; } = "Standard";
    
    [StringLength(20)]
    public string MeasurementUnit { get; set; } = "cm";
    
    [Required]
    [MinLength(1, ErrorMessage = "En az bir beden girişi gereklidir.")]
    public List<CreateSizeGuideEntryDto> Entries { get; set; } = new();
}
