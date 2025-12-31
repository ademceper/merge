using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class UpdateCurrencyDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Para birimi adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Sembol en az 1, en fazla 10 karakter olmalıdır.")]
    public string Symbol { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Döviz kuru 0 veya daha büyük olmalıdır.")]
    public decimal ExchangeRate { get; set; }
    
    public bool IsActive { get; set; }
    
    [Range(0, 10, ErrorMessage = "Ondalık basamak sayısı 0 ile 10 arasında olmalıdır.")]
    public int DecimalPlaces { get; set; }
    
    [StringLength(50)]
    public string Format { get; set; } = string.Empty;
}
