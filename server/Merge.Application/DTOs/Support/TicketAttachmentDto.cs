namespace Merge.Application.DTOs.Support;


public record TicketAttachmentDto(
    Guid Id,
    string FileName,
    string FilePath,
    string FileType,
    long FileSize,
    DateTime CreatedAt
);
