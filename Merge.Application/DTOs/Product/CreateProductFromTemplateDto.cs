using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class CreateProductFromTemplateDto
{
    [Required]
    public Guid TemplateId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Ürün adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string ProductName { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? SKU { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? Price { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır.")]
    public int? StockQuantity { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? ImageUrl { get; set; }
    
    public Guid? SellerId { get; set; }
    
    public Guid? StoreId { get; set; }
    
    public Dictionary<string, string>? AdditionalSpecifications { get; set; }
    
    public Dictionary<string, string>? AdditionalAttributes { get; set; }
    
    public List<string>? ImageUrls { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "İndirimli fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? DiscountPrice { get; set; }
}
