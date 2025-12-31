using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Organization;

public class UpdateTeamMemberDto
{
    [StringLength(50)]
    public string? Role { get; set; }
    
    public bool? IsActive { get; set; }
}
