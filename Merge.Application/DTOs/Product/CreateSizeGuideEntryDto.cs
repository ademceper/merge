using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class CreateSizeGuideEntryDto
{
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Beden etiketi gereklidir.")]
    public string SizeLabel { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? AlternativeLabel { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Ölçü değerleri 0 veya daha büyük olmalıdır.")]
    public decimal? Chest { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Waist { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Hips { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Inseam { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Shoulder { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Length { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Width { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Height { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Weight { get; set; }
    
    public Dictionary<string, string>? AdditionalMeasurements { get; set; }
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }
}
