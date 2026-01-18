namespace Merge.Application.DTOs.B2B;

public class B2BUserDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? UsedCredit { get; set; }
    public decimal? AvailableCredit { get; set; }
    
    // Typed DTO kullanılıyor
    public B2BUserSettingsDto? Settings { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
