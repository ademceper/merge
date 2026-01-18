using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Seller;

public record SellerOnboardingStatsDto
{
    public int TotalApplications { get; init; }
    public int PendingApplications { get; init; }
    public int ApprovedApplications { get; init; }
    public int RejectedApplications { get; init; }
    public int ApprovedThisMonth { get; init; }
    public decimal ApprovalRate { get; init; }
}
