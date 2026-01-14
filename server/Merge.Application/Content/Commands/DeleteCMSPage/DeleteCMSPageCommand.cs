using MediatR;

namespace Merge.Application.Content.Commands.DeleteCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteCMSPageCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

