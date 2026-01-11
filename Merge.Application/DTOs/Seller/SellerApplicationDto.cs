using Merge.Domain.Entities;
using Merge.Domain.Enums;
namespace Merge.Application.DTOs.Seller;

public class SellerApplicationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    // ✅ ARCHITECTURE: Enum kullanımı (string BusinessType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public BusinessType BusinessType { get; set; }
    public string TaxNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BusinessDescription { get; set; } = string.Empty;
    public decimal EstimatedMonthlyRevenue { get; set; }
    public SellerApplicationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
