using MediatR;

namespace Merge.Application.Content.Commands.PublishCMSPage;

public record PublishCMSPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

