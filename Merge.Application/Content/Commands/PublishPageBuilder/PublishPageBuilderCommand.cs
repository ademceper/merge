using MediatR;

namespace Merge.Application.Content.Commands.PublishPageBuilder;

public record PublishPageBuilderCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

