using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class CreateComparisonDto
{
    [StringLength(200)]
    public string? Name { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "En az 2 ürün karşılaştırılmalıdır.")]
    public List<Guid> ProductIds { get; set; } = new();
}
