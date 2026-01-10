using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Entities;

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
    // ✅ BOLUM 1.3: Value Objects - Email backing field (EF Core compatibility)
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

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private EmailSubscriber() { }

    // ✅ BOLUM 1.3: Value Object properties
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Email EmailValue => new Email(_email);

    // ✅ BOLUM 1.1: Factory Method with validation
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

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        subscriber.AddDomainEvent(new EmailSubscriberCreatedEvent(subscriber.Id, email.Value));

        return subscriber;
    }

    // ✅ BOLUM 1.1: Domain Method - Subscribe
    public void Subscribe()
    {
        if (IsSubscribed)
            return;

        IsSubscribed = true;
        SubscribedAt = DateTime.UtcNow;
        UnsubscribedAt = null;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailSubscriberSubscribedEvent(Id, Email));
    }

    // ✅ BOLUM 1.1: Domain Method - Unsubscribe
    public void Unsubscribe()
    {
        if (!IsSubscribed)
            return;

        IsSubscribed = false;
        UnsubscribedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailSubscriberUnsubscribedEvent(Id, Email));
    }

    // ✅ BOLUM 1.1: Domain Method - Update details
    public void UpdateDetails(
        string? firstName = null,
        string? lastName = null,
        string? source = null,
        string? tags = null,
        string? customFields = null)
    {
        if (firstName != null)
            FirstName = firstName;

        if (lastName != null)
            LastName = lastName;

        if (source != null)
            Source = source;

        if (tags != null)
            Tags = tags;

        if (customFields != null)
            CustomFields = customFields;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailSubscriberUpdatedEvent(Id, Email));
    }

    // ✅ BOLUM 1.1: Domain Method - Record email sent
    public void RecordEmailSent()
    {
        EmailsSent++;
        LastEmailSentAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Record email opened
    public void RecordEmailOpened()
    {
        EmailsOpened++;
        LastEmailOpenedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Record email clicked
    public void RecordEmailClicked()
    {
        EmailsClicked++;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        IsSubscribed = false;
        UnsubscribedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailSubscriberDeletedEvent(Id, Email));
    }
}

