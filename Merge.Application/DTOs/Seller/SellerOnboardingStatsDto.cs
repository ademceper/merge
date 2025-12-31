using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Seller;

public class SellerOnboardingStatsDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int ApprovedThisMonth { get; set; }
    public decimal ApprovalRate { get; set; }
}
