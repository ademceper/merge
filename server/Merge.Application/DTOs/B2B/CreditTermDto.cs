namespace Merge.Application.DTOs.B2B;

public class CreditTermDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int PaymentDays { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? UsedCredit { get; set; }
    public decimal? AvailableCredit { get; set; }
    public bool IsActive { get; set; }
    public string? Terms { get; set; }
    public DateTime CreatedAt { get; set; }
}
