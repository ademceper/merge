using MediatR;

namespace Merge.Application.Security.Commands.ResolveAlert;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ResolveAlertCommand(
    Guid AlertId,
    Guid ResolvedByUserId,
    string? ResolutionNotes = null
) : IRequest<bool>;
