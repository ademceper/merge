namespace Merge.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty; // Order, Payment, Shipping, Promotion
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? Link { get; set; } // Bildirime tıklandığında gidilecek link
    public string? Data { get; set; } // JSON formatında ek veriler
    
    // Navigation properties
    public User User { get; set; } = null!;
}

