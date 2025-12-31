using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Organization;

public class AddTeamMemberDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Member";
}
