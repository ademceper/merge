using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Seller;

public record SellerApplicationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string BusinessName { get; init; } = string.Empty;
    public BusinessType BusinessType { get; init; }
    public string TaxNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string BusinessDescription { get; init; } = string.Empty;
    public decimal EstimatedMonthlyRevenue { get; init; }
    public SellerApplicationStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
}
