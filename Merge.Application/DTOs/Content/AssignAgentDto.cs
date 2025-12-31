using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class AssignAgentDto
{
    [Required]
    public Guid AgentId { get; set; }
}
