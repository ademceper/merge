namespace Merge.Domain.Entities;

/// <summary>
/// LiveChatMessage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public LiveChatSession Session { get; set; } = null!;
    public Guid? SenderId { get; set; } // User or Agent ID
    public User? Sender { get; set; }
    public string SenderType { get; set; } = string.Empty; // User, Agent, System
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text"; // Text, Image, File, System
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? FileUrl { get; set; } // For file attachments
    public string? FileName { get; set; }
    public bool IsInternal { get; set; } = false; // Internal notes visible only to agents
}

