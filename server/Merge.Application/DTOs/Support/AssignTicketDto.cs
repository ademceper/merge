using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;


public record AssignTicketDto
{
    [Required]
    public Guid AssignedToId { get; init; }
}
