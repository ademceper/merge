using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record SellerOnboardingStatsDto
{
    public int TotalApplications { get; init; }
    public int PendingApplications { get; init; }
    public int ApprovedApplications { get; init; }
    public int RejectedApplications { get; init; }
    public int ApprovedThisMonth { get; init; }
    public decimal ApprovalRate { get; init; }
}
