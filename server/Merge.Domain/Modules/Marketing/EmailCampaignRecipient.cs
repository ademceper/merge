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

    private EmailCampaignRecipient() { }

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

        recipient.AddDomainEvent(new EmailCampaignRecipientCreatedEvent(recipient.Id, campaignId, subscriberId));

        return recipient;
    }

    public void MarkAsSent()
    {
        if (Status != EmailRecipientStatus.Pending)
            throw new DomainException("Sadece bekleyen alıcılar gönderilebilir");

        Status = EmailRecipientStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailCampaignRecipientSentEvent(Id, CampaignId, SubscriberId));
    }

    public void MarkAsDelivered()
    {
        Status = EmailRecipientStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailCampaignRecipientDeliveredEvent(Id, CampaignId, SubscriberId));
    }

    public void RecordEmailOpened()
    {
        var isFirstOpen = OpenedAt is null;
        
        if (isFirstOpen)
        {
            OpenedAt = DateTime.UtcNow;
            Status = EmailRecipientStatus.Opened;
        }
        
        OpenCount++;
        UpdatedAt = DateTime.UtcNow;

        if (isFirstOpen)
        {
            AddDomainEvent(new EmailCampaignRecipientOpenedEvent(Id, CampaignId, SubscriberId));
        }
    }

    public void RecordEmailClicked()
    {
        var isFirstClick = ClickedAt is null;
        
        if (isFirstClick)
        {
            ClickedAt = DateTime.UtcNow;
            Status = EmailRecipientStatus.Clicked;
        }
        
        ClickCount++;
        UpdatedAt = DateTime.UtcNow;

        if (isFirstClick)
        {
            AddDomainEvent(new EmailCampaignRecipientClickedEvent(Id, CampaignId, SubscriberId));
        }
    }

    public void MarkAsBounced(string? errorMessage = null)
    {
        Status = EmailRecipientStatus.Bounced;
        BouncedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailCampaignRecipientBouncedEvent(Id, CampaignId, SubscriberId, errorMessage));
    }

    public void MarkAsUnsubscribed()
    {
        Status = EmailRecipientStatus.Unsubscribed;
        UnsubscribedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailCampaignRecipientUnsubscribedEvent(Id, CampaignId, SubscriberId));
    }

    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));

        Status = EmailRecipientStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailCampaignRecipientFailedEvent(Id, CampaignId, SubscriberId, errorMessage));
    }

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
}

