namespace Merge.Application.DTOs.Analytics;

public class PopularPostDto
{
    public Guid PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
}
