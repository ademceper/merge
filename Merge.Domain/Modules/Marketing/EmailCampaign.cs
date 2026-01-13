using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Notifications;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// EmailCampaign Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailCampaign : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string FromName { get; private set; } = string.Empty;
    public string FromEmail { get; private set; } = string.Empty;
    public string ReplyToEmail { get; private set; } = string.Empty;
    public Guid? TemplateId { get; private set; }
    public EmailTemplate? Template { get; private set; }
    public string Content { get; private set; } = string.Empty; // HTML content
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public EmailCampaignStatus Status { get; private set; } = EmailCampaignStatus.Draft;
    public EmailCampaignType Type { get; private set; } = EmailCampaignType.Promotional;
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string TargetSegment { get; private set; } = string.Empty; // All, Active, Inactive, Buyers, etc.
    
    private int _totalRecipients = 0;
    public int TotalRecipients 
    { 
        get => _totalRecipients; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(TotalRecipients));
            _totalRecipients = value;
        }
    }
    
    private int _sentCount = 0;
    public int SentCount 
    { 
        get => _sentCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(SentCount));
            _sentCount = value;
        }
    }
    
    private int _deliveredCount = 0;
    public int DeliveredCount 
    { 
        get => _deliveredCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(DeliveredCount));
            _deliveredCount = value;
        }
    }
    
    private int _openedCount = 0;
    public int OpenedCount 
    { 
        get => _openedCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(OpenedCount));
            _openedCount = value;
        }
    }
    
    private int _clickedCount = 0;
    public int ClickedCount 
    { 
        get => _clickedCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ClickedCount));
            _clickedCount = value;
        }
    }
    
    private int _bouncedCount = 0;
    public int BouncedCount 
    { 
        get => _bouncedCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(BouncedCount));
            _bouncedCount = value;
        }
    }
    
    private int _unsubscribedCount = 0;
    public int UnsubscribedCount 
    { 
        get => _unsubscribedCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(UnsubscribedCount));
            _unsubscribedCount = value;
        }
    }
    
    private decimal _openRate = 0;
    public decimal OpenRate 
    { 
        get => _openRate; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(OpenRate));
            if (value > 100)
                throw new DomainException("Açılma oranı %100'den fazla olamaz");
            _openRate = value;
        }
    }
    
    private decimal _clickRate = 0;
    public decimal ClickRate 
    { 
        get => _clickRate; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ClickRate));
            if (value > 100)
                throw new DomainException("Tıklama oranı %100'den fazla olamaz");
            _clickRate = value;
        }
    }
    
    public string? Tags { get; private set; } // JSON array of tags
    public ICollection<EmailCampaignRecipient> Recipients { get; private set; } = new List<EmailCampaignRecipient>();

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private EmailCampaign() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static EmailCampaign Create(
        string name,
        string subject,
        string fromName,
        string fromEmail,
        string replyToEmail,
        string content,
        EmailCampaignType type,
        string targetSegment,
        DateTime? scheduledAt = null,
        Guid? templateId = null,
        string? tags = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(subject, nameof(subject));
        Guard.AgainstNullOrEmpty(fromName, nameof(fromName));
        Guard.AgainstNullOrEmpty(fromEmail, nameof(fromEmail));
        Guard.AgainstNullOrEmpty(replyToEmail, nameof(replyToEmail));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Guard.AgainstNullOrEmpty(targetSegment, nameof(targetSegment));

        var campaign = new EmailCampaign
        {
            Id = Guid.NewGuid(),
            Name = name,
            Subject = subject,
            FromName = fromName,
            FromEmail = fromEmail,
            ReplyToEmail = replyToEmail,
            Content = content,
            Type = type,
            TargetSegment = targetSegment,
            ScheduledAt = scheduledAt,
            TemplateId = templateId,
            Tags = tags,
            Status = EmailCampaignStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignCreatedEvent
        campaign.AddDomainEvent(new EmailCampaignCreatedEvent(campaign.Id, campaign.Name, campaign.Type));

        return campaign;
    }

    // ✅ BOLUM 1.1: Domain Method - Update details
    public void UpdateDetails(
        string name,
        string subject,
        string fromName,
        string fromEmail,
        string replyToEmail,
        string content,
        string? tags = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(subject, nameof(subject));
        Guard.AgainstNullOrEmpty(fromName, nameof(fromName));
        Guard.AgainstNullOrEmpty(fromEmail, nameof(fromEmail));
        Guard.AgainstNullOrEmpty(replyToEmail, nameof(replyToEmail));
        Guard.AgainstNullOrEmpty(content, nameof(content));

        Name = name;
        Subject = subject;
        FromName = fromName;
        FromEmail = fromEmail;
        ReplyToEmail = replyToEmail;
        Content = content;
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignUpdatedEvent
        AddDomainEvent(new EmailCampaignUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Set template ID
    public void SetTemplateId(Guid? templateId)
    {
        TemplateId = templateId;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignUpdatedEvent
        AddDomainEvent(new EmailCampaignUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Set target segment
    public void SetTargetSegment(string targetSegment)
    {
        Guard.AgainstNullOrEmpty(targetSegment, nameof(targetSegment));
        TargetSegment = targetSegment;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignUpdatedEvent
        AddDomainEvent(new EmailCampaignUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Schedule campaign
    public void Schedule(DateTime scheduledAt)
    {
        if (scheduledAt <= DateTime.UtcNow)
            throw new DomainException("Planlanan tarih gelecekte olmalıdır");

        if (Status != EmailCampaignStatus.Draft)
            throw new DomainException("Sadece taslak kampanyalar planlanabilir");

        ScheduledAt = scheduledAt;
        Status = EmailCampaignStatus.Scheduled;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignScheduledEvent
        AddDomainEvent(new EmailCampaignScheduledEvent(Id, Name, scheduledAt));
    }

    // ✅ BOLUM 1.1: Domain Method - Start sending
    public void StartSending(int totalRecipients)
    {
        if (Status != EmailCampaignStatus.Scheduled && Status != EmailCampaignStatus.Draft)
            throw new DomainException("Sadece planlanmış veya taslak kampanyalar gönderilebilir");

        Status = EmailCampaignStatus.Sending;
        TotalRecipients = totalRecipients;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignStartedEvent
        AddDomainEvent(new EmailCampaignStartedEvent(Id, Name, totalRecipients));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as sent
    public void MarkAsSent(int sentCount)
    {
        if (Status != EmailCampaignStatus.Sending)
            throw new DomainException("Sadece gönderilmekte olan kampanyalar tamamlanabilir");

        Status = EmailCampaignStatus.Sent;
        SentAt = DateTime.UtcNow;
        SentCount = sentCount;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignSentEvent
        AddDomainEvent(new EmailCampaignSentEvent(Id, Name, sentCount));
    }

    // ✅ BOLUM 1.1: Domain Method - Update statistics
    public void UpdateStatistics(
        int deliveredCount,
        int openedCount,
        int clickedCount,
        int bouncedCount,
        int unsubscribedCount)
    {
        DeliveredCount = deliveredCount;
        OpenedCount = openedCount;
        ClickedCount = clickedCount;
        BouncedCount = bouncedCount;
        UnsubscribedCount = unsubscribedCount;

        // Calculate rates
        if (DeliveredCount > 0)
        {
            OpenRate = (decimal)OpenedCount / DeliveredCount * 100;
            ClickRate = (decimal)ClickedCount / DeliveredCount * 100;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Pause campaign
    public void Pause()
    {
        if (Status != EmailCampaignStatus.Sending)
            throw new DomainException("Sadece gönderilmekte olan kampanyalar duraklatılabilir");

        Status = EmailCampaignStatus.Paused;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignPausedEvent
        AddDomainEvent(new EmailCampaignPausedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Cancel campaign
    public void Cancel()
    {
        if (Status == EmailCampaignStatus.Sent)
            throw new DomainException("Gönderilmiş kampanyalar iptal edilemez");

        Status = EmailCampaignStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignCancelledEvent
        AddDomainEvent(new EmailCampaignCancelledEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as failed
    public void MarkAsFailed()
    {
        Status = EmailCampaignStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignFailedEvent
        AddDomainEvent(new EmailCampaignFailedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        if (Status == EmailCampaignStatus.Sending)
            throw new DomainException("Gönderilmekte olan kampanyalar silinemez");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignDeletedEvent
        AddDomainEvent(new EmailCampaignDeletedEvent(Id, Name));
    }
}
