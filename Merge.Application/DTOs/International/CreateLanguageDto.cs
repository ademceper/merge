using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class CreateLanguageDto
{
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Yerel dil adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string NativeName { get; set; } = string.Empty;
    
    public bool IsDefault { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsRTL { get; set; } = false;
    
    [StringLength(200)]
    public string FlagIcon { get; set; } = string.Empty;
}
