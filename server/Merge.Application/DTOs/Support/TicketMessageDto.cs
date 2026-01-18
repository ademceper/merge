namespace Merge.Application.DTOs.Support;


public record TicketMessageDto(
    Guid Id,
    Guid TicketId,
    Guid UserId,
    string UserName,
    string Message,
    bool IsStaffResponse,
    bool IsInternal,
    DateTime CreatedAt,
    IReadOnlyList<TicketAttachmentDto> Attachments
);
