using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Governance;

/// <summary>
/// Policy güncelleme DTO - BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
/// BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
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
