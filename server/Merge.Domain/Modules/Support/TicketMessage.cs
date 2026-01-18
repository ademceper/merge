using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// TicketMessage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TicketMessage : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Guid UserId { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsStaffResponse { get; private set; } = false;
    public bool IsInternal { get; private set; } = false;

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public SupportTicket Ticket { get; private set; } = null!;
    public User User { get; private set; } = null!;
    
    private readonly List<TicketAttachment> _attachments = new();
    public IReadOnlyCollection<TicketAttachment> Attachments => _attachments.AsReadOnly();

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private TicketMessage() { }

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

        ticketMessage.AddDomainEvent(new TicketMessageAddedEvent(
            ticketMessage.Id,
            ticketId,
            ticketNumber,
            userId,
            isStaffResponse));

        return ticketMessage;
    }

    public void UpdateMessage(string message)
    {
        Guard.AgainstNullOrEmpty(message, nameof(message));
        // Configuration değeri: MaxTicketMessageLength=10000
        Guard.AgainstLength(message, 10000, nameof(message));

        Message = message;
        UpdatedAt = DateTime.UtcNow;
    }
}

