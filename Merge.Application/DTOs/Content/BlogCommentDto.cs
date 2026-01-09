namespace Merge.Application.DTOs.Content;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
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
