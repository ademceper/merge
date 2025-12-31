using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class CreateShippingDto
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kargo sağlayıcısı adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string ShippingProvider { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Kargo maliyeti 0 veya daha büyük olmalıdır.")]
    public decimal ShippingCost { get; set; }
}
