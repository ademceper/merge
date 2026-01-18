using MediatR;

namespace Merge.Application.Security.Commands.ResolveAlert;

public record ResolveAlertCommand(
    Guid AlertId,
    Guid ResolvedByUserId,
    string? ResolutionNotes = null
) : IRequest<bool>;
