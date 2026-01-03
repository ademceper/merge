using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SupportTicket Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SupportTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty; // Auto-generated: TKT-XXXXXX
    public Guid UserId { get; set; }
    public TicketCategory Category { get; set; } = TicketCategory.Other;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; } // Optional: related order
    public Guid? ProductId { get; set; } // Optional: related product
    public Guid? AssignedToId { get; set; } // Admin/Support agent assigned
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int ResponseCount { get; set; } = 0;
    public DateTime? LastResponseAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Order? Order { get; set; }
    public Product? Product { get; set; }
    public User? AssignedTo { get; set; }
    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}
