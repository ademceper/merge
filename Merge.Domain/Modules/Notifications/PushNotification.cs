using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Notifications;

/// <summary>
/// PushNotification Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PushNotification : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid? UserId { get; private set; } // Null for broadcast notifications
    public Guid? DeviceId { get; private set; } // Specific device, null for all user devices
    
    private string _title = string.Empty;
    public string Title 
    { 
        get => _title; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Title));
            Guard.AgainstLength(value, 200, nameof(Title));
            _title = value;
        }
    }
    
    private string _body = string.Empty;
    public string Body 
    { 
        get => _body; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Body));
            Guard.AgainstLength(value, 1000, nameof(Body));
            _body = value;
        }
    }
    
    private string? _imageUrl;
    public string? ImageUrl 
    { 
        get => _imageUrl; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 500, nameof(ImageUrl));
            }
            _imageUrl = value;
        }
    }
    
    public string? Data { get; private set; } // JSON data payload
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status yerine)
    public CommunicationStatus Status { get; private set; } = CommunicationStatus.Pending;
    
    // ✅ BOLUM 1.2: Enum kullanımı (string NotificationType yerine)
    public NotificationType NotificationType { get; private set; }
    
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    
    private string? _errorMessage;
    public string? ErrorMessage 
    { 
        get => _errorMessage; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 500, nameof(ErrorMessage));
            }
            _errorMessage = value;
        }
    }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User? User { get; private set; }
    public PushNotificationDevice? Device { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PushNotification() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PushNotification Create(
        NotificationType notificationType,
        string title,
        string body,
        Guid? userId = null,
        Guid? deviceId = null,
        string? imageUrl = null,
        string? data = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstNullOrEmpty(body, nameof(body));
        Guard.AgainstLength(body, 1000, nameof(body));
        if (imageUrl != null)
        {
            Guard.AgainstLength(imageUrl, 500, nameof(imageUrl));
        }

        var pushNotification = new PushNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeviceId = deviceId,
            _title = title,
            _body = body,
            _imageUrl = imageUrl,
            Data = data,
            NotificationType = notificationType,
            Status = CommunicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - PushNotificationCreatedEvent
        pushNotification.AddDomainEvent(new PushNotificationCreatedEvent(
            pushNotification.Id, 
            userId, 
            deviceId, 
            notificationType, 
            title));

        return pushNotification;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as sent
    public void MarkAsSent()
    {
        if (Status == CommunicationStatus.Sent)
            throw new DomainException("Push notification zaten gönderilmiş");

        Status = CommunicationStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PushNotificationSentEvent
        AddDomainEvent(new PushNotificationSentEvent(Id, UserId, SentAt.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as delivered
    public void MarkAsDelivered()
    {
        if (Status == CommunicationStatus.Delivered)
            throw new DomainException("Push notification zaten teslim edilmiş");

        if (Status != CommunicationStatus.Sent)
            throw new DomainException("Push notification önce gönderilmelidir");

        Status = CommunicationStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PushNotificationDeliveredEvent
        AddDomainEvent(new PushNotificationDeliveredEvent(Id, UserId, DeliveredAt.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as failed
    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));
        Guard.AgainstLength(errorMessage, 500, nameof(errorMessage));

        Status = CommunicationStatus.Failed;
        _errorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PushNotificationFailedEvent
        AddDomainEvent(new PushNotificationFailedEvent(Id, UserId, errorMessage));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as bounced
    public void MarkAsBounced()
    {
        Status = CommunicationStatus.Bounced;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PushNotificationBouncedEvent
        AddDomainEvent(new PushNotificationBouncedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Delete notification (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Push notification zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PushNotificationDeletedEvent
        AddDomainEvent(new PushNotificationDeletedEvent(Id, UserId));
    }
}

