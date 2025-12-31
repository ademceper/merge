using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Support;

public class TicketMessageDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsStaffResponse { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TicketAttachmentDto> Attachments { get; set; } = new();
}
