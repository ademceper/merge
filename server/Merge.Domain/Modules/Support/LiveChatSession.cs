using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// LiveChatSession Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveChatSession : BaseEntity, IAggregateRoot
{
    public Guid? UserId { get; private set; }
    public Guid? AgentId { get; private set; }
    public string SessionId { get; private set; } = string.Empty;
    public ChatSessionStatus Status { get; private set; } = ChatSessionStatus.Waiting;
    public string? GuestName { get; private set; }
    public string? GuestEmail { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public DateTime? StartedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public int MessageCount { get; private set; } = 0;
    public int UnreadCount { get; private set; } = 0;
    public string? Department { get; private set; }
    
    private int _priority = 0;
    public int Priority 
    { 
        get => _priority; 
        private set 
        {
            Guard.AgainstOutOfRange(value, 0, 2, nameof(Priority));
            _priority = value;
        } 
    }
    
    public string? Tags { get; private set; }

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public User? User { get; private set; }
    public User? Agent { get; private set; }
    
    private readonly List<LiveChatMessage> _messages = new();
    public IReadOnlyCollection<LiveChatMessage> Messages => _messages.AsReadOnly();

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private LiveChatSession() { }

    public static LiveChatSession Create(
        string sessionId,
        Guid? userId = null,
        string? guestName = null,
        string? guestEmail = null,
        string? department = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        Guard.AgainstNullOrEmpty(sessionId, nameof(sessionId));

        var session = new LiveChatSession
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            UserId = userId,
            GuestName = guestName,
            GuestEmail = guestEmail,
            Department = department,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
            Status = ChatSessionStatus.Waiting,
            StartedAt = DateTime.UtcNow,
            Priority = 0,
            MessageCount = 0,
            UnreadCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        session.AddDomainEvent(new LiveChatSessionCreatedEvent(
            session.Id,
            session.SessionId,
            userId,
            guestName,
            department));

        return session;
    }

    public void AssignAgent(Guid agentId)
    {
        Guard.AgainstDefault(agentId, nameof(agentId));

        if (AgentId.HasValue)
            throw new DomainException("Session zaten bir ajana atanmış");

        AgentId = agentId;
        Status = ChatSessionStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LiveChatSessionAssignedEvent(Id, SessionId, agentId));
    }

    public void Close()
    {
        if (Status == ChatSessionStatus.Closed)
            throw new DomainException("Session zaten kapatılmış");

        var oldStatus = Status;
        Status = ChatSessionStatus.Closed;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LiveChatSessionClosedEvent(Id, SessionId, UserId, ResolvedAt.Value));
    }

    public void UpdatePriority(int priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMessage(string senderType)
    {
        MessageCount++;
        UpdatedAt = DateTime.UtcNow;

        if (senderType == "User")
        {
            UnreadCount++;
        }
        else if (senderType == "Agent")
        {
            UnreadCount = 0; // Reset when agent responds
        }

        if (Status == ChatSessionStatus.Waiting && senderType == "Agent")
        {
            Status = ChatSessionStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkMessagesAsRead()
    {
        UnreadCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Chat session zaten silinmiş");

        IsDeleted = true;
        if (Status != ChatSessionStatus.Closed)
        {
            Status = ChatSessionStatus.Closed;
            ResolvedAt = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LiveChatSessionDeletedEvent(Id, SessionId, UserId, AgentId));
    }

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }
}

