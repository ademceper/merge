using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

public class CreateVolumeDiscountDto
{
    public Guid? ProductId { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    public Guid? OrganizationId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Minimum miktar en az 1 olmalıdır.")]
    public int MinQuantity { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Maksimum miktar en az 1 olmalıdır.")]
    public int? MaxQuantity { get; set; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "İndirim yüzdesi 0 ile 100 arasında olmalıdır.")]
    public decimal DiscountPercentage { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Sabit indirim tutarı 0 veya daha büyük olmalıdır.")]
    public decimal? FixedDiscountAmount { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}
