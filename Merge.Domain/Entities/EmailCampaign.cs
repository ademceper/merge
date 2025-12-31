namespace Merge.Domain.Entities;

public class EmailCampaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string ReplyToEmail { get; set; } = string.Empty;
    public Guid? TemplateId { get; set; }
    public EmailTemplate? Template { get; set; }
    public string Content { get; set; } = string.Empty; // HTML content
    public EmailCampaignStatus Status { get; set; } = EmailCampaignStatus.Draft;
    public EmailCampaignType Type { get; set; } = EmailCampaignType.Promotional;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string TargetSegment { get; set; } = string.Empty; // All, Active, Inactive, Buyers, etc.
    public int TotalRecipients { get; set; } = 0;
    public int SentCount { get; set; } = 0;
    public int DeliveredCount { get; set; } = 0;
    public int OpenedCount { get; set; } = 0;
    public int ClickedCount { get; set; } = 0;
    public int BouncedCount { get; set; } = 0;
    public int UnsubscribedCount { get; set; } = 0;
    public decimal OpenRate { get; set; } = 0;
    public decimal ClickRate { get; set; } = 0;
    public string? Tags { get; set; } // JSON array of tags
    public ICollection<EmailCampaignRecipient> Recipients { get; set; } = new List<EmailCampaignRecipient>();
}

public class EmailTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string TextContent { get; set; } = string.Empty;
    public EmailTemplateType Type { get; set; } = EmailTemplateType.Custom;
    public bool IsActive { get; set; } = true;
    public string? Thumbnail { get; set; }
    public string? Variables { get; set; } // JSON array of available variables like {{customer_name}}, {{order_number}}
    public ICollection<EmailCampaign> Campaigns { get; set; } = new List<EmailCampaign>();
}

public class EmailSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public bool IsSubscribed { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }
    public string? Source { get; set; } // Checkout, Newsletter Form, Import, etc.
    public string? Tags { get; set; } // JSON array
    public string? CustomFields { get; set; } // JSON object for additional data
    public int EmailsSent { get; set; } = 0;
    public int EmailsOpened { get; set; } = 0;
    public int EmailsClicked { get; set; } = 0;
    public DateTime? LastEmailSentAt { get; set; }
    public DateTime? LastEmailOpenedAt { get; set; }
}

public class EmailCampaignRecipient : BaseEntity
{
    public Guid CampaignId { get; set; }
    public EmailCampaign Campaign { get; set; } = null!;
    public Guid SubscriberId { get; set; }
    public EmailSubscriber Subscriber { get; set; } = null!;
    public EmailRecipientStatus Status { get; set; } = EmailRecipientStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public DateTime? BouncedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int OpenCount { get; set; } = 0;
    public int ClickCount { get; set; } = 0;
}

public class EmailAutomation : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EmailAutomationType Type { get; set; } = EmailAutomationType.WelcomeSeries;
    public bool IsActive { get; set; } = true;
    public Guid TemplateId { get; set; }
    public EmailTemplate Template { get; set; } = null!;
    public int DelayHours { get; set; } = 0; // Delay before sending
    public string? TriggerConditions { get; set; } // JSON object defining when to trigger
    public int TotalTriggered { get; set; } = 0;
    public int TotalSent { get; set; } = 0;
    public DateTime? LastTriggeredAt { get; set; }
}

public enum EmailCampaignStatus
{
    Draft,
    Scheduled,
    Sending,
    Sent,
    Paused,
    Cancelled,
    Failed
}

public enum EmailCampaignType
{
    Promotional,
    Transactional,
    Newsletter,
    Announcement,
    AbandonedCart,
    WelcomeSeries,
    ProductRecommendation,
    WinBack,
    Seasonal,
    Survey
}

public enum EmailTemplateType
{
    Custom,
    Welcome,
    OrderConfirmation,
    ShippingUpdate,
    DeliveryConfirmation,
    PasswordReset,
    AccountActivation,
    Newsletter,
    Promotional,
    AbandonedCart,
    ProductRecommendation,
    ReviewRequest,
    WinBack,
    Receipt
}

public enum EmailRecipientStatus
{
    Pending,
    Sent,
    Delivered,
    Opened,
    Clicked,
    Bounced,
    Failed,
    Unsubscribed
}

public enum EmailAutomationType
{
    WelcomeSeries,
    AbandonedCart,
    PostPurchase,
    WinBack,
    Birthday,
    ReviewRequest,
    ReorderReminder,
    LowStock,
    BackInStock
}
