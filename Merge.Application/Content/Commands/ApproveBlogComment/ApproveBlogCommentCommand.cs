using MediatR;

namespace Merge.Application.Content.Commands.ApproveBlogComment;

public record ApproveBlogCommentCommand(
    Guid Id
) : IRequest<bool>;

