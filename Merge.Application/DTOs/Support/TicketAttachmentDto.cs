namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record TicketAttachmentDto(
    Guid Id,
    string FileName,
    string FilePath,
    string FileType,
    long FileSize,
    DateTime CreatedAt
);
