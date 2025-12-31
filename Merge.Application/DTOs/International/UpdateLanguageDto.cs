using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class UpdateLanguageDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Yerel dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string NativeName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public bool IsRTL { get; set; }
    
    [StringLength(200)]
    public string FlagIcon { get; set; } = string.Empty;
}
