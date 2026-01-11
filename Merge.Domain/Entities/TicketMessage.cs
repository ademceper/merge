using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

/// <summary>
/// TicketMessage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TicketMessage : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid TicketId { get; private set; }
    public Guid UserId { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsStaffResponse { get; private set; } = false;
    public bool IsInternal { get; private set; } = false;

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public SupportTicket Ticket { get; private set; } = null!;
    public User User { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<TicketAttachment> _attachments = new();
    public IReadOnlyCollection<TicketAttachment> Attachments => _attachments.AsReadOnly();

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private TicketMessage() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static TicketMessage Create(
        Guid ticketId,
        string ticketNumber,
        Guid userId,
        string message,
        bool isStaffResponse = false,
        bool isInternal = false)
    {
        Guard.AgainstDefault(ticketId, nameof(ticketId));
        Guard.AgainstNullOrEmpty(ticketNumber, nameof(ticketNumber));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(message, nameof(message));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxTicketMessageLength=10000
        Guard.AgainstLength(message, 10000, nameof(message));

        var ticketMessage = new TicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UserId = userId,
            Message = message,
            IsStaffResponse = isStaffResponse,
            IsInternal = isInternal,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - TicketMessageAddedEvent
        ticketMessage.AddDomainEvent(new TicketMessageAddedEvent(
            ticketMessage.Id,
            ticketId,
            ticketNumber,
            userId,
            isStaffResponse));

        return ticketMessage;
    }

    // ✅ BOLUM 1.1: Domain Method - Update message
    public void UpdateMessage(string message)
    {
        Guard.AgainstNullOrEmpty(message, nameof(message));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxTicketMessageLength=10000
        Guard.AgainstLength(message, 10000, nameof(message));

        Message = message;
        UpdatedAt = DateTime.UtcNow;
    }
}

