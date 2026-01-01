namespace Merge.Application.DTOs.Product;

public class ProductComparisonDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // ✅ SECURITY: IDOR koruması için gerekli
    public string Name { get; set; } = string.Empty;
    public bool IsSaved { get; set; }
    public string? ShareCode { get; set; }
    public List<ComparisonProductDto> Products { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
