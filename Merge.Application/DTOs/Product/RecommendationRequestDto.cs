using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class RecommendationRequestDto
{
    public Guid? ProductId { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    [Range(1, 100, ErrorMessage = "Maksimum sonuç sayısı 1 ile 100 arasında olmalıdır.")]
    public int MaxResults { get; set; } = 10;
}
