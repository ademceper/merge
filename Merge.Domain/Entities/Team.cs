namespace Merge.Domain.Entities;

/// <summary>
/// Team Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Team : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? TeamLeadId { get; set; } // Team leader user ID
    public bool IsActive { get; set; } = true;
    public string? Settings { get; set; } // JSON for team-specific settings
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public User? TeamLead { get; set; }
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
}

