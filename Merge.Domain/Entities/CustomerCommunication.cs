using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

/// <summary>
/// CustomerCommunication Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CustomerCommunication : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
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

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CustomerCommunication() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
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

        // ✅ BOLUM 1.5: Domain Events - CustomerCommunicationCreatedEvent
        communication.AddDomainEvent(new CustomerCommunicationCreatedEvent(
            communication.Id,
            userId,
            communicationType,
            channel,
            direction));

        return communication;
    }

    // ✅ BOLUM 1.1: Domain Method - Update status
    public void UpdateStatus(CommunicationStatus newStatus, DateTime? deliveredAt = null, DateTime? readAt = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (deliveredAt.HasValue)
            DeliveredAt = deliveredAt.Value;

        if (readAt.HasValue)
            ReadAt = readAt.Value;

        // ✅ BOLUM 1.5: Domain Events - CustomerCommunicationStatusChangedEvent
        AddDomainEvent(new CustomerCommunicationStatusChangedEvent(
            Id,
            oldStatus.ToString(),
            newStatus.ToString()));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as delivered
    public void MarkAsDelivered()
    {
        if (Status == CommunicationStatus.Delivered)
            return;

        UpdateStatus(CommunicationStatus.Delivered, DateTime.UtcNow);
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as read
    public void MarkAsRead()
    {
        if (Status == CommunicationStatus.Read)
            return;

        UpdateStatus(CommunicationStatus.Read, DeliveredAt, DateTime.UtcNow);
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as failed
    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));

        ErrorMessage = errorMessage;
        UpdateStatus(CommunicationStatus.Failed);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }
}

