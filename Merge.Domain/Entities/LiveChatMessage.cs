using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

/// <summary>
/// LiveChatMessage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveChatMessage : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid SessionId { get; private set; }
    public Guid? SenderId { get; private set; }
    public string SenderType { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string MessageType { get; private set; } = "Text";
    public bool IsRead { get; private set; } = false;
    public DateTime? ReadAt { get; private set; }
    public string? FileUrl { get; private set; }
    public string? FileName { get; private set; }
    public bool IsInternal { get; private set; } = false;

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public LiveChatSession Session { get; private set; } = null!;
    public User? Sender { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LiveChatMessage() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LiveChatMessage Create(
        Guid sessionId,
        string sessionIdentifier,
        Guid? senderId,
        string senderType,
        string content,
        string messageType = "Text",
        string? fileUrl = null,
        string? fileName = null,
        bool isInternal = false)
    {
        Guard.AgainstDefault(sessionId, nameof(sessionId));
        Guard.AgainstNullOrEmpty(sessionIdentifier, nameof(sessionIdentifier));
        Guard.AgainstNullOrEmpty(senderType, nameof(senderType));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxChatMessageLength=10000
        Guard.AgainstLength(content, 10000, nameof(content));

        if (senderType != "User" && senderType != "Agent" && senderType != "System")
            throw new DomainException("SenderType must be User, Agent, or System");

        var message = new LiveChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            SenderId = senderId,
            SenderType = senderType,
            Content = content,
            MessageType = messageType,
            FileUrl = fileUrl,
            FileName = fileName,
            IsInternal = isInternal,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - LiveChatMessageSentEvent
        message.AddDomainEvent(new LiveChatMessageSentEvent(
            message.Id,
            sessionId,
            sessionIdentifier,
            senderId,
            senderType));

        return message;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as read
    public void MarkAsRead()
    {
        if (IsRead) return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update content
    public void UpdateContent(string content)
    {
        Guard.AgainstNullOrEmpty(content, nameof(content));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxChatMessageLength=10000
        Guard.AgainstLength(content, 10000, nameof(content));

        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}

