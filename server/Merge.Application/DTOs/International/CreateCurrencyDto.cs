using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CreateCurrencyDto(
    [Required]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.")]
    string Code,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Para birimi adı en az 2, en fazla 100 karakter olmalıdır.")]
    string Name,
    
    [Required]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Sembol en az 1, en fazla 10 karakter olmalıdır.")]
    string Symbol,
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Döviz kuru 0 veya daha büyük olmalıdır.")]
    decimal ExchangeRate,
    
    bool IsBaseCurrency = false,
    
    bool IsActive = true,
    
    [Range(0, 10, ErrorMessage = "Ondalık basamak sayısı 0 ile 10 arasında olmalıdır.")]
    int DecimalPlaces = 2,
    
    [StringLength(50)]
    string Format = "{symbol}{amount}");
