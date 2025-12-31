namespace Merge.Application.DTOs.Content;

public class BlogCommentDto
{
    public Guid Id { get; set; }
    public Guid BlogPostId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
    public List<BlogCommentDto> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
