using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class UpdateSupportTicketDto
{
    [StringLength(200)]
    public string? Subject { get; set; }
    
    [StringLength(5000)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    [StringLength(20)]
    public string? Priority { get; set; }
    
    [StringLength(50)]
    public string? Status { get; set; }
    
    public Guid? AssignedToId { get; set; }
}
