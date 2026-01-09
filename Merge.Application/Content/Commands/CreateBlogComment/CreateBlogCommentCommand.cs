using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateBlogComment;

public record CreateBlogCommentCommand(
    Guid BlogPostId,
    string Content,
    Guid? UserId = null, // Controller'dan set edilecek (authenticated user için)
    Guid? ParentCommentId = null,
    string? AuthorName = null, // Guest comment için
    string? AuthorEmail = null // Guest comment için
) : IRequest<BlogCommentDto>;

