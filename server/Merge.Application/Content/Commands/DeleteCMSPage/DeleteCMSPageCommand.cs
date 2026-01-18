using MediatR;

namespace Merge.Application.Content.Commands.DeleteCMSPage;

public record DeleteCMSPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

