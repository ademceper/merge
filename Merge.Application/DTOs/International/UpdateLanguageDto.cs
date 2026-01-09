using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record UpdateLanguageDto(
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    string Name,
    
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Yerel dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    string NativeName,
    
    bool IsActive,
    
    bool IsRTL,
    
    [StringLength(200)]
    string FlagIcon);
