using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

public class AddProductToStreamDto
{
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsHighlighted { get; set; } = false;
    
    [Range(0, double.MaxValue, ErrorMessage = "Özel fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? SpecialPrice { get; set; }
    
    [StringLength(500)]
    public string? ShowcaseNotes { get; set; }
}
