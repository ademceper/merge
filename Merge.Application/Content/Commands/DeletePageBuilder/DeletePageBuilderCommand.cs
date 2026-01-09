using MediatR;

namespace Merge.Application.Content.Commands.DeletePageBuilder;

public record DeletePageBuilderCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

