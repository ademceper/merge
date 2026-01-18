using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Content;

namespace Merge.Application.DTOs.Governance;


public record UpdatePolicyDto(
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    string? Title = null,
    
    [StringLength(50000)]
    string? Content = null,
    
    [StringLength(20)]
    string? Version = null,
    
    bool? IsActive = null,
    
    bool? RequiresAcceptance = null,
    
    DateTime? EffectiveDate = null,
    
    DateTime? ExpiryDate = null,
    
    [StringLength(2000)]
    string? ChangeLog = null);
