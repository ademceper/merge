using MediatR;

namespace Merge.Application.Content.Commands.SetHomePageCMSPage;

public record SetHomePageCMSPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

