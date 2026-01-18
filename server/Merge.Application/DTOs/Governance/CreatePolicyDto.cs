using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Content;

namespace Merge.Application.DTOs.Governance;


public record CreatePolicyDto(
    [Required]
    [StringLength(100)]
    string PolicyType,
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    string Title,
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    string Content,
    
    [StringLength(20)]
    string Version = "1.0",
    
    bool IsActive = true,
    
    bool RequiresAcceptance = true,
    
    DateTime? EffectiveDate = null,
    
    DateTime? ExpiryDate = null,
    
    [StringLength(2000)]
    string? ChangeLog = null,
    
    [StringLength(10)]
    string Language = "tr");
