namespace Merge.Domain.Entities;

public enum TicketStatus
{
    Open,
    InProgress,
    Waiting,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum TicketCategory
{
    Order,
    Product,
    Payment,
    Shipping,
    Return,
    Account,
    Technical,
    Other
}

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
