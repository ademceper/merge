using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

/// <summary>
/// SupportTicket Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SupportTicket : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string TicketNumber { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public TicketCategory Category { get; private set; } = TicketCategory.Other;
    public TicketPriority Priority { get; private set; } = TicketPriority.Medium;
    public TicketStatus Status { get; private set; } = TicketStatus.Open;
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid? OrderId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? AssignedToId { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public int ResponseCount { get; private set; } = 0;
    public DateTime? LastResponseAt { get; private set; }

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public User User { get; private set; } = null!;
    public Order? Order { get; private set; }
    public Product? Product { get; private set; }
    public User? AssignedTo { get; private set; }
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<TicketMessage> _messages = new();
    public IReadOnlyCollection<TicketMessage> Messages => _messages.AsReadOnly();
    
    private readonly List<TicketAttachment> _attachments = new();
    public IReadOnlyCollection<TicketAttachment> Attachments => _attachments.AsReadOnly();

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SupportTicket() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SupportTicket Create(
        string ticketNumber,
        Guid userId,
        TicketCategory category,
        TicketPriority priority,
        string subject,
        string description,
        Guid? orderId = null,
        Guid? productId = null)
    {
        Guard.AgainstNullOrEmpty(ticketNumber, nameof(ticketNumber));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(subject, nameof(subject));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxCommunicationSubjectLength=200, MaxTicketMessageLength=10000
        Guard.AgainstLength(subject, 200, nameof(subject));
        Guard.AgainstLength(description, 5000, nameof(description));

        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            UserId = userId,
            Category = category,
            Priority = priority,
            Status = TicketStatus.Open,
            Subject = subject,
            Description = description,
            OrderId = orderId,
            ProductId = productId,
            ResponseCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - SupportTicketCreatedEvent
        ticket.AddDomainEvent(new SupportTicketCreatedEvent(
            ticket.Id,
            ticket.TicketNumber,
            ticket.UserId,
            ticket.Category.ToString(),
            ticket.Priority.ToString(),
            ticket.Subject));

        return ticket;
    }

    // ✅ BOLUM 1.1: Domain Method - Assign ticket
    public void AssignTo(Guid assignedToId)
    {
        Guard.AgainstDefault(assignedToId, nameof(assignedToId));

        var oldStatus = Status;
        AssignedToId = assignedToId;

        if (Status == TicketStatus.Open)
        {
            Status = TicketStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;

            // ✅ BOLUM 1.5: Domain Events - SupportTicketStatusChangedEvent
            AddDomainEvent(new SupportTicketStatusChangedEvent(
                Id,
                TicketNumber,
                oldStatus.ToString(),
                Status.ToString()));
        }

        // ✅ BOLUM 1.5: Domain Events - SupportTicketAssignedEvent
        AddDomainEvent(new SupportTicketAssignedEvent(Id, TicketNumber, assignedToId));
    }

    // ✅ BOLUM 1.1: Domain Method - Update status
    public void UpdateStatus(TicketStatus newStatus)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == TicketStatus.Resolved && ResolvedAt == null)
        {
            ResolvedAt = DateTime.UtcNow;
            // ✅ BOLUM 1.5: Domain Events - SupportTicketResolvedEvent
            AddDomainEvent(new SupportTicketResolvedEvent(Id, TicketNumber, UserId, ResolvedAt.Value));
        }
        else if (newStatus == TicketStatus.Closed && ClosedAt == null)
        {
            ClosedAt = DateTime.UtcNow;
            // ✅ BOLUM 1.5: Domain Events - SupportTicketClosedEvent
            AddDomainEvent(new SupportTicketClosedEvent(Id, TicketNumber, UserId, ClosedAt.Value));
        }

        // ✅ BOLUM 1.5: Domain Events - SupportTicketStatusChangedEvent
        AddDomainEvent(new SupportTicketStatusChangedEvent(
            Id,
            TicketNumber,
            oldStatus.ToString(),
            newStatus.ToString()));
    }

    // ✅ BOLUM 1.1: Domain Method - Update priority
    public void UpdatePriority(TicketPriority priority)
    {
        if (Priority == priority) return;

        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update category
    public void UpdateCategory(TicketCategory category)
    {
        if (Category == category) return;

        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update subject
    public void UpdateSubject(string subject)
    {
        Guard.AgainstNullOrEmpty(subject, nameof(subject));
        Guard.AgainstLength(subject, 200, nameof(subject));

        Subject = subject;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update description
    public void UpdateDescription(string description)
    {
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstLength(description, 5000, nameof(description));

        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Add message
    public void AddMessage()
    {
        ResponseCount++;
        LastResponseAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update status when message added
    public void UpdateStatusOnMessage(bool isStaffResponse)
    {
        if (Status == TicketStatus.Waiting && isStaffResponse)
        {
            UpdateStatus(TicketStatus.InProgress);
        }
        else if (!isStaffResponse && Status != TicketStatus.Waiting)
        {
            UpdateStatus(TicketStatus.Waiting);
        }
    }

    // ✅ BOLUM 1.1: Domain Method - Resolve ticket
    public void Resolve()
    {
        if (Status == TicketStatus.Resolved || Status == TicketStatus.Closed)
            throw new DomainException("Bilet zaten çözülmüş veya kapatılmış");

        UpdateStatus(TicketStatus.Resolved);
    }

    // ✅ BOLUM 1.1: Domain Method - Close ticket
    public void Close()
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Bilet zaten kapatılmış");

        UpdateStatus(TicketStatus.Closed);
    }

    // ✅ BOLUM 1.1: Domain Method - Reopen ticket
    public void Reopen()
    {
        if (Status != TicketStatus.Closed && Status != TicketStatus.Resolved)
            throw new DomainException("Sadece kapatılmış veya çözülmüş biletler yeniden açılabilir");

        var oldStatus = Status;
        Status = TicketStatus.Open;
        ClosedAt = null;
        ResolvedAt = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - SupportTicketStatusChangedEvent
        AddDomainEvent(new SupportTicketStatusChangedEvent(
            Id,
            TicketNumber,
            oldStatus.ToString(),
            Status.ToString()));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Bilet zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - SupportTicketDeletedEvent
        AddDomainEvent(new SupportTicketDeletedEvent(Id, TicketNumber, UserId));
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }
}
