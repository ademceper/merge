using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class CreateFraudDetectionRuleDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string RuleType { get; set; } = string.Empty;
    
    /// Typed DTO (Over-posting korumasi)
    public FraudRuleConditionsDto? Conditions { get; set; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "Risk skoru 0 ile 100 arasında olmalıdır.")]
    public int RiskScore { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = "Flag";
    
    public bool IsActive { get; set; } = true;
    
    [Range(0, int.MaxValue)]
    public int Priority { get; set; } = 0;
    
    [StringLength(1000)]
    public string? Description { get; set; }
}
