using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Content;

namespace Merge.Application.DTOs.Governance;


public record AcceptPolicyDto(
    [Required(ErrorMessage = "Policy ID zorunludur")]
    Guid PolicyId);
