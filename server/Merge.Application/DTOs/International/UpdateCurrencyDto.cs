using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public record UpdateCurrencyDto(
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Para birimi adı en az 2, en fazla 100 karakter olmalıdır.")]
    string Name,
    
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Sembol en az 1, en fazla 10 karakter olmalıdır.")]
    string Symbol,
    
    [Range(0, double.MaxValue, ErrorMessage = "Döviz kuru 0 veya daha büyük olmalıdır.")]
    decimal ExchangeRate,
    
    bool IsActive,
    
    [Range(0, 10, ErrorMessage = "Ondalık basamak sayısı 0 ile 10 arasında olmalıdır.")]
    int DecimalPlaces,
    
    [StringLength(50)]
    string Format);
