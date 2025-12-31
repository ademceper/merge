using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class UpdateProductTemplateDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Şablon adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Name { get; set; }
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    [StringLength(100)]
    public string? Brand { get; set; }
    
    [StringLength(50)]
    public string? DefaultSKUPrefix { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Varsayılan fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? DefaultPrice { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Varsayılan stok miktarı 0 veya daha büyük olmalıdır.")]
    public int? DefaultStockQuantity { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? DefaultImageUrl { get; set; }
    
    public Dictionary<string, string>? Specifications { get; set; }
    
    public Dictionary<string, string>? Attributes { get; set; }
    
    public bool? IsActive { get; set; }
}
