using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Organization;

public class CreateTeamDto
{
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Takım adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public Guid? TeamLeadId { get; set; }
    
    public Dictionary<string, object>? Settings { get; set; }
}
