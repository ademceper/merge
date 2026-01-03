namespace Merge.Domain.Entities;

/// <summary>
/// TicketMessage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TicketMessage : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsStaffResponse { get; set; } = false;
    public bool IsInternal { get; set; } = false; // Internal notes not visible to customer

    // Navigation properties
    public SupportTicket Ticket { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}

