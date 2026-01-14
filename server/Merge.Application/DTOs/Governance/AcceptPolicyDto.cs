using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Content;

namespace Merge.Application.DTOs.Governance;

/// <summary>
/// Policy kabul DTO - BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
/// BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public record AcceptPolicyDto(
    [Required(ErrorMessage = "Policy ID zorunludur")]
    Guid PolicyId);
