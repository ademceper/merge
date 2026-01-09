using MediatR;

namespace Merge.Application.Content.Commands.PublishCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record PublishCMSPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

