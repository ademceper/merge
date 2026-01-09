using MediatR;

namespace Merge.Application.Content.Commands.PublishLandingPage;

public record PublishLandingPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

