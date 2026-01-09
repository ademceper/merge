using MediatR;

namespace Merge.Application.Content.Commands.PublishBlogPost;

public record PublishBlogPostCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

