namespace Merge.Application.DTOs.Content;

public record BlogCommentDto(
    Guid Id,
    Guid BlogPostId,
    Guid? UserId,
    string? UserName,
    Guid? ParentCommentId,
    string AuthorName,
    string Content,
    bool IsApproved,
    int LikeCount,
    int ReplyCount,
    IReadOnlyList<BlogCommentDto>? Replies,
    DateTime CreatedAt
);
