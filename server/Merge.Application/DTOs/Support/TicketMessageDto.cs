namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
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
