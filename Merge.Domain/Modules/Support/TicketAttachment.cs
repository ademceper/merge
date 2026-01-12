using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// TicketAttachment Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TicketAttachment : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid TicketId { get; private set; }
    public Guid? MessageId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public SupportTicket Ticket { get; private set; } = null!;
    public TicketMessage? Message { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private TicketAttachment() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static TicketAttachment Create(
        Guid ticketId,
        string fileName,
        string filePath,
        string fileType,
        long fileSize,
        Guid? messageId = null)
    {
        Guard.AgainstDefault(ticketId, nameof(ticketId));
        Guard.AgainstNullOrEmpty(fileName, nameof(fileName));
        Guard.AgainstNullOrEmpty(filePath, nameof(filePath));
        Guard.AgainstNullOrEmpty(fileType, nameof(fileType));
        Guard.AgainstNegativeOrZero(fileSize, nameof(fileSize));
        Guard.AgainstLength(fileName, 255, nameof(fileName));

        return new TicketAttachment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            MessageId = messageId,
            FileName = fileName,
            FilePath = filePath,
            FileType = fileType,
            FileSize = fileSize,
            CreatedAt = DateTime.UtcNow
        };
    }
}
