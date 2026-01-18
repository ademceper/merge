using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// EmailSubscriber Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Email Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailSubscriber : BaseEntity, IAggregateRoot
{
    private string _email = string.Empty;
    public string Email 
    { 
        get => _email; 
        private set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Email boş olamaz");
            
            // Email validation
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new DomainException("Geçersiz e-posta adresi formatı");
            
            _email = value.ToLowerInvariant();
        } 
    }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public bool IsSubscribed { get; private set; } = true;
    public DateTime SubscribedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; private set; }
    public string? Source { get; private set; } // Checkout, Newsletter Form, Import, etc.
    public string? Tags { get; private set; } // JSON array
    public string? CustomFields { get; private set; } // JSON object for additional data
    
    private int _emailsSent = 0;
    public int EmailsSent 
    { 
        get => _emailsSent; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(EmailsSent));
            _emailsSent = value;
        }
    }
    
    private int _emailsOpened = 0;
    public int EmailsOpened 
    { 
        get => _emailsOpened; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(EmailsOpened));
            _emailsOpened = value;
        }
    }
    
    private int _emailsClicked = 0;
    public int EmailsClicked 
    { 
        get => _emailsClicked; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(EmailsClicked));
            _emailsClicked = value;
        }
    }
    
    public DateTime? LastEmailSentAt { get; private set; }
    public DateTime? LastEmailOpenedAt { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private EmailSubscriber() { }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Email EmailValue => new Email(_email);

    public static EmailSubscriber Create(
        Email email,
        string? firstName = null,
        string? lastName = null,
        Guid? userId = null,
        string? source = null,
        string? tags = null,
        string? customFields = null)
    {
        Guard.AgainstNull(email, nameof(email));

        var subscriber = new EmailSubscriber
        {
            _email = email.Value,
            FirstName = firstName,
            LastName = lastName,
            UserId = userId,
            Source = source,
            Tags = tags,
            CustomFields = customFields,
            IsSubscribed = true,
            SubscribedAt = DateTime.UtcNow
        };

        subscriber.AddDomainEvent(new EmailSubscriberCreatedEvent(subscriber.Id, email.Value));

        return subscriber;
    }

    public void Subscribe()
    {
        if (IsSubscribed)
            return;

        IsSubscribed = true;
        SubscribedAt = DateTime.UtcNow;
        UnsubscribedAt = null;

        AddDomainEvent(new EmailSubscriberSubscribedEvent(Id, Email));
    }

    public void Unsubscribe()
    {
        if (!IsSubscribed)
            return;

        IsSubscribed = false;
        UnsubscribedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailSubscriberUnsubscribedEvent(Id, Email));
    }

    public void UpdateDetails(
        string? firstName = null,
        string? lastName = null,
        string? source = null,
        string? tags = null,
        string? customFields = null)
    {
        if (firstName is not null)
            FirstName = firstName;

        if (lastName is not null)
            LastName = lastName;

        if (source is not null)
            Source = source;

        if (tags is not null)
            Tags = tags;

        if (customFields is not null)
            CustomFields = customFields;

        AddDomainEvent(new EmailSubscriberUpdatedEvent(Id, Email));
    }

    public void RecordEmailSent()
    {
        EmailsSent++;
        LastEmailSentAt = DateTime.UtcNow;
    }

    public void RecordEmailOpened()
    {
        EmailsOpened++;
        LastEmailOpenedAt = DateTime.UtcNow;
    }

    public void RecordEmailClicked()
    {
        EmailsClicked++;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        IsSubscribed = false;
        UnsubscribedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailSubscriberDeletedEvent(Id, Email));
    }
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

