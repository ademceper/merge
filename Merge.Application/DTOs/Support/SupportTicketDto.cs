using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Support;

public class SupportTicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int ResponseCount { get; set; }
    public DateTime? LastResponseAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TicketMessageDto> Messages { get; set; } = new();
    public List<TicketAttachmentDto> Attachments { get; set; } = new();
}
