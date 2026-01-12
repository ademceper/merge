using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Organization;

public class AddTeamMemberDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Member";
}
