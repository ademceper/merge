using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Governance;

public class UpdatePolicyDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Title { get; set; }
    
    [StringLength(50000)]
    public string? Content { get; set; }
    
    [StringLength(20)]
    public string? Version { get; set; }
    
    public bool? IsActive { get; set; }
    
    public bool? RequiresAcceptance { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [StringLength(2000)]
    public string? ChangeLog { get; set; }
}
