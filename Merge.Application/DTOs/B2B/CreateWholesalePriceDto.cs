using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

public class CreateWholesalePriceDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    public Guid? OrganizationId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Minimum miktar en az 1 olmalıdır.")]
    public int MinQuantity { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Maksimum miktar en az 1 olmalıdır.")]
    public int? MaxQuantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal Price { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}
