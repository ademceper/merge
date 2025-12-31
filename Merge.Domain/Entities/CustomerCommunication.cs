namespace Merge.Domain.Entities;

public class CustomerCommunication : BaseEntity
{
    public Guid UserId { get; set; }
    public string CommunicationType { get; set; } = string.Empty; // Email, SMS, Phone, Ticket, Chat, InApp
    public string Channel { get; set; } = string.Empty; // Support, Order, Marketing, System
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Direction { get; set; } = "Outbound"; // Inbound, Outbound
    public Guid? RelatedEntityId { get; set; } // OrderId, TicketId, etc.
    public string? RelatedEntityType { get; set; } // Order, Ticket, etc.
    public Guid? SentByUserId { get; set; } // Staff/System who sent
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public string Status { get; set; } = "Sent"; // Sent, Delivered, Read, Failed, Bounced
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; } // JSON for additional data
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User? SentBy { get; set; }
}

