using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Search;

public class SearchRequestDto
{
    [StringLength(200)]
    public string? SearchTerm { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    [StringLength(100)]
    public string? Brand { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Minimum fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? MinPrice { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Maksimum fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? MaxPrice { get; set; }
    
    [Range(0, 5, ErrorMessage = "Minimum puan 0 ile 5 arasında olmalıdır.")]
    public decimal? MinRating { get; set; }
    
    public bool InStockOnly { get; set; } = false;
    
    [StringLength(50)]
    public string? SortBy { get; set; } // price_asc, price_desc, rating, newest, popular
    
    [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası en az 1 olmalıdır.")]
    public int? Page { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "Sayfa boyutu 1 ile 100 arasında olmalıdır.")]
    public int? PageSize { get; set; } = 20;
}
