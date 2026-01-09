using MediatR;

namespace Merge.Application.Content.Commands.DeleteLandingPage;

public record DeleteLandingPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

