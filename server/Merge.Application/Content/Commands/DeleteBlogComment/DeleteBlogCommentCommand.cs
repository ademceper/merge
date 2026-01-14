using MediatR;

namespace Merge.Application.Content.Commands.DeleteBlogComment;

public record DeleteBlogCommentCommand(
    Guid Id
) : IRequest<bool>;

