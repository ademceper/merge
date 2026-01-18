using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// CustomerCommunication Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CustomerCommunication : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string CommunicationType { get; private set; } = string.Empty;
    public string Channel { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Direction { get; private set; } = "Outbound";
    public Guid? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? SentByUserId { get; private set; }
    public string? RecipientEmail { get; private set; }
    public string? RecipientPhone { get; private set; }
    public CommunicationStatus Status { get; private set; } = CommunicationStatus.Sent;
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? Metadata { get; private set; }

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public User User { get; private set; } = null!;
    public User? SentBy { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private CustomerCommunication() { }

    public static CustomerCommunication Create(
        Guid userId,
        string communicationType,
        string channel,
        string subject,
        string content,
        string direction = "Outbound",
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        Guid? sentByUserId = null,
        string? recipientEmail = null,
        string? recipientPhone = null,
        string? metadata = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(communicationType, nameof(communicationType));
        Guard.AgainstNullOrEmpty(channel, nameof(channel));
        Guard.AgainstNullOrEmpty(subject, nameof(subject));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        // Configuration değerleri: MaxCommunicationSubjectLength=200, MaxCommunicationContentLength=10000
        Guard.AgainstLength(subject, 200, nameof(subject));
        Guard.AgainstLength(content, 10000, nameof(content));

        if (direction != "Inbound" && direction != "Outbound")
            throw new DomainException("Direction must be Inbound or Outbound");

        var communication = new CustomerCommunication
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CommunicationType = communicationType,
            Channel = channel,
            Subject = subject,
            Content = content,
            Direction = direction,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            SentByUserId = sentByUserId,
            RecipientEmail = recipientEmail,
            RecipientPhone = recipientPhone,
            Status = CommunicationStatus.Sent,
            SentAt = DateTime.UtcNow,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        communication.AddDomainEvent(new CustomerCommunicationCreatedEvent(
            communication.Id,
            userId,
            communicationType,
            channel,
            direction));

        return communication;
    }

    public void UpdateStatus(CommunicationStatus newStatus, DateTime? deliveredAt = null, DateTime? readAt = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (deliveredAt.HasValue)
            DeliveredAt = deliveredAt.Value;

        if (readAt.HasValue)
            ReadAt = readAt.Value;

        AddDomainEvent(new CustomerCommunicationStatusChangedEvent(
            Id,
            oldStatus.ToString(),
            newStatus.ToString()));
    }

    public void MarkAsDelivered()
    {
        if (Status == CommunicationStatus.Delivered)
            return;

        UpdateStatus(CommunicationStatus.Delivered, DateTime.UtcNow);
    }

    public void MarkAsRead()
    {
        if (Status == CommunicationStatus.Read)
            return;

        UpdateStatus(CommunicationStatus.Read, DeliveredAt, DateTime.UtcNow);
    }

    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));

        ErrorMessage = errorMessage;
        UpdateStatus(CommunicationStatus.Failed);
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("İletişim kaydı zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerCommunicationDeletedEvent(Id, UserId, CommunicationType, Channel));
    }

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }
}

