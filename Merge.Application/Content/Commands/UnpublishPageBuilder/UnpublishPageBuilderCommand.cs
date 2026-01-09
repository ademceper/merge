using MediatR;

namespace Merge.Application.Content.Commands.UnpublishPageBuilder;

public record UnpublishPageBuilderCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

