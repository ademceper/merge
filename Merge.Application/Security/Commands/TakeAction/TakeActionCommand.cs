using MediatR;

namespace Merge.Application.Security.Commands.TakeAction;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record TakeActionCommand(
    Guid EventId,
    Guid ActionTakenByUserId,
    string Action,
    string? Notes = null
) : IRequest<bool>;
