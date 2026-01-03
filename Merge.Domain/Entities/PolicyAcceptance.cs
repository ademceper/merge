namespace Merge.Domain.Entities;

/// <summary>
/// PolicyAcceptance Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PolicyAcceptance : BaseEntity
{
    public Guid PolicyId { get; set; }
    public Guid UserId { get; set; }
    public string AcceptedVersion { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true; // False if user revoked acceptance
    
    // Navigation properties
    public Policy Policy { get; set; } = null!;
    public User User { get; set; } = null!;
}

