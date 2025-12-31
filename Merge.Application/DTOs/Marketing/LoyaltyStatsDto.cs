namespace Merge.Application.DTOs.Marketing;

public class LoyaltyStatsDto
{
    public int TotalMembers { get; set; }
    public long TotalPointsIssued { get; set; }
    public long TotalPointsRedeemed { get; set; }
    public Dictionary<string, int> MembersByTier { get; set; } = new();
    public decimal AveragePointsPerMember { get; set; }
}
