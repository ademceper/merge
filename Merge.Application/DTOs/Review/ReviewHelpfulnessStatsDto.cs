using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Review;

public class ReviewHelpfulnessStatsDto
{
    public Guid ReviewId { get; set; }
    public int HelpfulCount { get; set; }
    public int UnhelpfulCount { get; set; }
    public int TotalVotes { get; set; }
    public decimal HelpfulPercentage { get; set; }
    public bool? UserVote { get; set; } // null = no vote, true = helpful, false = not helpful
}
