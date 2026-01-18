using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Notifications;

/// <summary>
/// Notification Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Notification : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    
    public NotificationType Type { get; private set; }
    
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
    
    private string _message = string.Empty;
    public string Message 
    { 
        get => _message; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Message));
            Guard.AgainstLength(value, 2000, nameof(Message));
            _message = value;
        }
    }
    
    public bool IsRead { get; private set; } = false;
    public DateTime? ReadAt { get; private set; }
    
    private string? _link;
    public string? Link 
    { 
        get => _link; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 500, nameof(Link));
            }
            _link = value;
        }
    }
    
    public string? Data { get; private set; } // JSON formatında ek veriler
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    private Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? link = null,
        string? data = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstNullOrEmpty(message, nameof(message));
        Guard.AgainstLength(message, 2000, nameof(message));
        if (link != null)
        {
            Guard.AgainstLength(link, 500, nameof(link));
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            _title = title,
            _message = message,
            _link = link,
            Data = data,
            IsRead = false,
            ReadAt = null,
            CreatedAt = DateTime.UtcNow
        };

        notification.AddDomainEvent(new NotificationCreatedEvent(notification.Id, userId, type, title));

        return notification;
    }

    public void MarkAsRead()
    {
        if (IsRead)
            throw new DomainException("Bildirim zaten okunmuş");

        var readAt = DateTime.UtcNow;
        IsRead = true;
        ReadAt = readAt;
        UpdatedAt = readAt;

        AddDomainEvent(new NotificationReadEvent(Id, UserId, readAt));
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Bildirim zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationDeletedEvent(Id, UserId));
    }
}

