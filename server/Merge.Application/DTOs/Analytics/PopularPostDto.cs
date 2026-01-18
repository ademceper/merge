namespace Merge.Application.DTOs.Analytics;

public record PopularPostDto(
    Guid PostId,
    string Title,
    int ViewCount,
    int CommentCount
);
