namespace Merge.Domain.Entities;

/// <summary>
/// EmailSubscriber Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public bool IsSubscribed { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }
    public string? Source { get; set; } // Checkout, Newsletter Form, Import, etc.
    public string? Tags { get; set; } // JSON array
    public string? CustomFields { get; set; } // JSON object for additional data
    public int EmailsSent { get; set; } = 0;
    public int EmailsOpened { get; set; } = 0;
    public int EmailsClicked { get; set; } = 0;
    public DateTime? LastEmailSentAt { get; set; }
    public DateTime? LastEmailOpenedAt { get; set; }
}

