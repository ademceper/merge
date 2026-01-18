using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Review;

public record ReviewerStatsDto(
    Guid UserId,
    string UserName,
    int ReviewCount,
    decimal AverageRating,
    int HelpfulVotes
);
