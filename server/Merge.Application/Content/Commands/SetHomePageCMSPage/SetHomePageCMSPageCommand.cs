using MediatR;

namespace Merge.Application.Content.Commands.SetHomePageCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SetHomePageCMSPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

