using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

/// <summary>
/// Partial update DTO for Volume Discount (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchVolumeDiscountDto
{
    public Guid? ProductId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? OrganizationId { get; init; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Minimum miktar en az 1 olmalıdır.")]
    public int? MinQuantity { get; init; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Maksimum miktar en az 1 olmalıdır.")]
    public int? MaxQuantity { get; init; }
    
    [Range(0, 100, ErrorMessage = "İndirim yüzdesi 0 ile 100 arasında olmalıdır.")]
    public decimal? DiscountPercentage { get; init; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Sabit indirim tutarı 0 veya daha büyük olmalıdır.")]
    public decimal? FixedDiscountAmount { get; init; }
    
    public bool? IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
