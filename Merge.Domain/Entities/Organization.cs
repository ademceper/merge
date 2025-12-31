namespace Merge.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; } // Tax ID / VAT Number
    public string? RegistrationNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? Settings { get; set; } // JSON for organization-specific settings
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}

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

public class TeamMember : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member"; // Member, Lead, Admin
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}

