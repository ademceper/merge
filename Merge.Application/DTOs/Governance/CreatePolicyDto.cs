using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Governance;

public class CreatePolicyDto
{
    [Required]
    [StringLength(100)]
    public string PolicyType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Version { get; set; } = "1.0";
    
    public bool IsActive { get; set; } = true;
    
    public bool RequiresAcceptance { get; set; } = true;
    
    public DateTime? EffectiveDate { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [StringLength(2000)]
    public string? ChangeLog { get; set; }
    
    [StringLength(10)]
    public string Language { get; set; } = "tr";
}
