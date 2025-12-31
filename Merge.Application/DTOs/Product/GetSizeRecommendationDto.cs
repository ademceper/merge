using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class GetSizeRecommendationDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [Range(50, 250, ErrorMessage = "Boy 50 ile 250 cm arasında olmalıdır.")]
    public decimal Height { get; set; }
    
    [Required]
    [Range(20, 300, ErrorMessage = "Kilo 20 ile 300 kg arasında olmalıdır.")]
    public decimal Weight { get; set; }
    
    [Range(50, 200, ErrorMessage = "Göğüs ölçüsü 50 ile 200 cm arasında olmalıdır.")]
    public decimal? Chest { get; set; }
    
    [Range(50, 200, ErrorMessage = "Bel ölçüsü 50 ile 200 cm arasında olmalıdır.")]
    public decimal? Waist { get; set; }
}
