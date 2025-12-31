namespace Merge.Domain.Entities;

public class CMSPage : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // HTML/Markdown content
    public string? Excerpt { get; set; }
    public string PageType { get; set; } = "Page"; // Page, Landing, Custom
    public string Status { get; set; } = "Draft"; // Draft, Published, Archived
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Template { get; set; } // Template name for rendering
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsHomePage { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public bool ShowInMenu { get; set; } = true;
    public string? MenuTitle { get; set; } // Different title for menu display
    public Guid? ParentPageId { get; set; } // For hierarchical pages
    public CMSPage? ParentPage { get; set; }
    public ICollection<CMSPage> ChildPages { get; set; } = new List<CMSPage>();
    public int ViewCount { get; set; } = 0;
}

public class LandingPage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // JSON or HTML content
    public string? Template { get; set; } // Template identifier
    public string Status { get; set; } = "Draft"; // Draft, Published, Archived
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? StartDate { get; set; } // When to start showing
    public DateTime? EndDate { get; set; } // When to stop showing
    public bool IsActive { get; set; } = true;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public int ViewCount { get; set; } = 0;
    public int ConversionCount { get; set; } = 0; // Track conversions
    public decimal ConversionRate { get; set; } = 0; // Percentage
    public bool EnableABTesting { get; set; } = false;
    public Guid? VariantOfId { get; set; } // If this is a variant for A/B testing
    public LandingPage? VariantOf { get; set; }
    public ICollection<LandingPage> Variants { get; set; } = new List<LandingPage>();
    public int TrafficSplit { get; set; } = 50; // Percentage of traffic for A/B testing
}

public class LiveChatSession : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable for guest chats
    public User? User { get; set; }
    public Guid? AgentId { get; set; } // Assigned support agent
    public User? Agent { get; set; }
    public string SessionId { get; set; } = string.Empty; // Unique session identifier
    public string Status { get; set; } = "Waiting"; // Waiting, Active, Resolved, Closed
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

public class LiveChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public LiveChatSession Session { get; set; } = null!;
    public Guid? SenderId { get; set; } // User or Agent ID
    public User? Sender { get; set; }
    public string SenderType { get; set; } = string.Empty; // User, Agent, System
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text"; // Text, Image, File, System
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? FileUrl { get; set; } // For file attachments
    public string? FileName { get; set; }
    public bool IsInternal { get; set; } = false; // Internal notes visible only to agents
}

public class FraudDetectionRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // Order, Payment, Account, Behavior
    public string Conditions { get; set; } = string.Empty; // JSON string for rule conditions
    public int RiskScore { get; set; } = 0; // Risk score if rule matches (0-100)
    public string Action { get; set; } = "Flag"; // Flag, Block, Review, Alert
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Higher priority rules checked first
    public string? Description { get; set; }
}

public class FraudAlert : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public string AlertType { get; set; } = string.Empty; // Order, Payment, Account, Behavior
    public int RiskScore { get; set; } = 0; // Calculated risk score (0-100)
    public string Status { get; set; } = "Pending"; // Pending, Reviewed, Resolved, FalsePositive
    public string? Reason { get; set; } // Why this alert was triggered
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public string? MatchedRules { get; set; } // JSON array of matched rule IDs
}

