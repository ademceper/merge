namespace Merge.Domain.Entities;

/// <summary>
/// TicketAttachment Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TicketAttachment : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid? MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    // Navigation properties
    public SupportTicket Ticket { get; set; } = null!;
    public TicketMessage? Message { get; set; }
}

