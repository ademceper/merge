using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class CreateCurrencyDto
{
    [Required]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.")]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Para birimi adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Sembol en az 1, en fazla 10 karakter olmalıdır.")]
    public string Symbol { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Döviz kuru 0 veya daha büyük olmalıdır.")]
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    public bool IsBaseCurrency { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    [Range(0, 10, ErrorMessage = "Ondalık basamak sayısı 0 ile 10 arasında olmalıdır.")]
    public int DecimalPlaces { get; set; } = 2;
    
    [StringLength(50)]
    public string Format { get; set; } = "{symbol}{amount}";
}
