using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CreateLanguageDto(
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    string Code,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    string Name,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Yerel dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    string NativeName,
    
    bool IsDefault = false,
    
    bool IsActive = true,
    
    bool IsRTL = false,
    
    [StringLength(200)]
    string FlagIcon = "");
