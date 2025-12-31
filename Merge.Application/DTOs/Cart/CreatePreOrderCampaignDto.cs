using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

public class CreatePreOrderCampaignDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kampanya adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public DateTime ExpectedDeliveryDate { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum miktar 0 veya daha büyük olmalıdır.")]
    public int MaxQuantity { get; set; } = 0;
    
    [Range(0, 100, ErrorMessage = "Depozito yüzdesi 0 ile 100 arasında olmalıdır.")]
    public decimal DepositPercentage { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Özel fiyat 0 veya daha büyük olmalıdır.")]
    public decimal SpecialPrice { get; set; } = 0;
}
