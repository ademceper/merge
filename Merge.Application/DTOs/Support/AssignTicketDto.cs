using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Support;

public class AssignTicketDto
{
    [Required]
    public Guid AssignedToId { get; set; }
}
