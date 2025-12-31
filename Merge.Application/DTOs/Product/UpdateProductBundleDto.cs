using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class UpdateProductBundleDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Paket adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Paket fiyatı 0 veya daha büyük olmalıdır.")]
    public decimal BundlePrice { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string ImageUrl { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}
