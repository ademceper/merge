using MediatR;

namespace Merge.Application.Content.Commands.DeleteBlogPost;

public record DeleteBlogPostCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

