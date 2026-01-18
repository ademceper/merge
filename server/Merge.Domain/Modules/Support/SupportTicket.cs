using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// SupportTicket Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SupportTicket : BaseEntity, IAggregateRoot
{
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
    
    private readonly List<TicketMessage> _messages = new();
    public IReadOnlyCollection<TicketMessage> Messages => _messages.AsReadOnly();
    
    private readonly List<TicketAttachment> _attachments = new();
    public IReadOnlyCollection<TicketAttachment> Attachments => _attachments.AsReadOnly();

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private SupportTicket() { }

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

        ticket.AddDomainEvent(new SupportTicketCreatedEvent(
            ticket.Id,
            ticket.TicketNumber,
            ticket.UserId,
            ticket.Category.ToString(),
            ticket.Priority.ToString(),
            ticket.Subject));

        return ticket;
    }

    public void AssignTo(Guid assignedToId)
    {
        Guard.AgainstDefault(assignedToId, nameof(assignedToId));

        var oldStatus = Status;
        AssignedToId = assignedToId;

        if (Status == TicketStatus.Open)
        {
            Status = TicketStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new SupportTicketStatusChangedEvent(
                Id,
                TicketNumber,
                oldStatus.ToString(),
                Status.ToString()));
        }

        AddDomainEvent(new SupportTicketAssignedEvent(Id, TicketNumber, assignedToId));
    }

    public void UpdateStatus(TicketStatus newStatus)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == TicketStatus.Resolved && ResolvedAt == null)
        {
            ResolvedAt = DateTime.UtcNow;
            AddDomainEvent(new SupportTicketResolvedEvent(Id, TicketNumber, UserId, ResolvedAt.Value));
        }
        else if (newStatus == TicketStatus.Closed && ClosedAt == null)
        {
            ClosedAt = DateTime.UtcNow;
            AddDomainEvent(new SupportTicketClosedEvent(Id, TicketNumber, UserId, ClosedAt.Value));
        }

        AddDomainEvent(new SupportTicketStatusChangedEvent(
            Id,
            TicketNumber,
            oldStatus.ToString(),
            newStatus.ToString()));
    }

    public void UpdatePriority(TicketPriority priority)
    {
        if (Priority == priority) return;

        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(TicketCategory category)
    {
        if (Category == category) return;

        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSubject(string subject)
    {
        Guard.AgainstNullOrEmpty(subject, nameof(subject));
        Guard.AgainstLength(subject, 200, nameof(subject));

        Subject = subject;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstLength(description, 5000, nameof(description));

        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMessage()
    {
        ResponseCount++;
        LastResponseAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

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

    public void Resolve()
    {
        if (Status == TicketStatus.Resolved || Status == TicketStatus.Closed)
            throw new DomainException("Bilet zaten çözülmüş veya kapatılmış");

        UpdateStatus(TicketStatus.Resolved);
    }

    public void Close()
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Bilet zaten kapatılmış");

        UpdateStatus(TicketStatus.Closed);
    }

    public void Reopen()
    {
        if (Status != TicketStatus.Closed && Status != TicketStatus.Resolved)
            throw new DomainException("Sadece kapatılmış veya çözülmüş biletler yeniden açılabilir");

        var oldStatus = Status;
        Status = TicketStatus.Open;
        ClosedAt = null;
        ResolvedAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SupportTicketStatusChangedEvent(
            Id,
            TicketNumber,
            oldStatus.ToString(),
            Status.ToString()));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Bilet zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SupportTicketDeletedEvent(Id, TicketNumber, UserId));
    }

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }
}
