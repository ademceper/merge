using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// LiveChatSession Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveChatSession : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable for guest chats
    public User? User { get; set; }
    public Guid? AgentId { get; set; } // Assigned support agent
    public User? Agent { get; set; }
    public string SessionId { get; set; } = string.Empty; // Unique session identifier
    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Waiting;
    public string? GuestName { get; set; } // For guest users
    public string? GuestEmail { get; set; } // For guest users
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int MessageCount { get; set; } = 0;
    public int UnreadCount { get; set; } = 0; // Unread messages for agent
    public string? Department { get; set; } // Department/category
    public int Priority { get; set; } = 0; // 0=Normal, 1=High, 2=Urgent
    public string? Tags { get; set; } // Comma separated tags
    
    // Navigation properties
    public ICollection<LiveChatMessage> Messages { get; set; } = new List<LiveChatMessage>();
}

