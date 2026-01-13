using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// EmailCampaignRecipient Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailCampaignRecipient : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CampaignId { get; private set; }
    public EmailCampaign Campaign { get; private set; } = null!;
    public Guid SubscriberId { get; private set; }
    public EmailSubscriber Subscriber { get; private set; } = null!;
    public EmailRecipientStatus Status { get; private set; } = EmailRecipientStatus.Pending;
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? OpenedAt { get; private set; }
    public DateTime? ClickedAt { get; private set; }
    public DateTime? BouncedAt { get; private set; }
    public DateTime? UnsubscribedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private int _openCount = 0;
    public int OpenCount 
    { 
        get => _openCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(OpenCount));
            _openCount = value;
        }
    }
    
    private int _clickCount = 0;
    public int ClickCount 
    { 
        get => _clickCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ClickCount));
            _clickCount = value;
        }
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Factory Method
    public static EmailCampaignRecipient Create(
        Guid campaignId,
        Guid subscriberId)
    {
        Guard.AgainstDefault(campaignId, nameof(campaignId));
        Guard.AgainstDefault(subscriberId, nameof(subscriberId));

        var recipient = new EmailCampaignRecipient
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            SubscriberId = subscriberId,
            Status = EmailRecipientStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientCreatedEvent
        recipient.AddDomainEvent(new EmailCampaignRecipientCreatedEvent(recipient.Id, campaignId, subscriberId));

        return recipient;
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method - Mark as sent
    public void MarkAsSent()
    {
        if (Status != EmailRecipientStatus.Pending)
            throw new DomainException("Sadece bekleyen alıcılar gönderilebilir");

        Status = EmailRecipientStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientSentEvent
        AddDomainEvent(new EmailCampaignRecipientSentEvent(Id, CampaignId, SubscriberId));
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method - Mark as delivered
    public void MarkAsDelivered()
    {
        Status = EmailRecipientStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientDeliveredEvent
        AddDomainEvent(new EmailCampaignRecipientDeliveredEvent(Id, CampaignId, SubscriberId));
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method - Record email opened
    public void RecordEmailOpened()
    {
        var isFirstOpen = OpenedAt == null;
        
        if (isFirstOpen)
        {
            OpenedAt = DateTime.UtcNow;
            Status = EmailRecipientStatus.Opened;
        }
        
        OpenCount++;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientOpenedEvent (sadece ilk açılışta)
        if (isFirstOpen)
        {
            AddDomainEvent(new EmailCampaignRecipientOpenedEvent(Id, CampaignId, SubscriberId));
        }
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method - Record email clicked
    public void RecordEmailClicked()
    {
        var isFirstClick = ClickedAt == null;
        
        if (isFirstClick)
        {
            ClickedAt = DateTime.UtcNow;
            Status = EmailRecipientStatus.Clicked;
        }
        
        ClickCount++;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientClickedEvent (sadece ilk tıklamada)
        if (isFirstClick)
        {
            AddDomainEvent(new EmailCampaignRecipientClickedEvent(Id, CampaignId, SubscriberId));
        }
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method - Mark as bounced
    public void MarkAsBounced(string? errorMessage = null)
    {
        Status = EmailRecipientStatus.Bounced;
        BouncedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientBouncedEvent
        AddDomainEvent(new EmailCampaignRecipientBouncedEvent(Id, CampaignId, SubscriberId, errorMessage));
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method - Mark as unsubscribed
    public void MarkAsUnsubscribed()
    {
        Status = EmailRecipientStatus.Unsubscribed;
        UnsubscribedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientUnsubscribedEvent
        AddDomainEvent(new EmailCampaignRecipientUnsubscribedEvent(Id, CampaignId, SubscriberId));
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method - Mark as failed
    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));

        Status = EmailRecipientStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - EmailCampaignRecipientFailedEvent
        AddDomainEvent(new EmailCampaignRecipientFailedEvent(Id, CampaignId, SubscriberId, errorMessage));
    }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Schema.Timestamp]
    public byte[]? RowVersion { get; set; }
}

