using MediatR;

namespace Merge.Application.Security.Commands.TakeAction;

public record TakeActionCommand(
    Guid EventId,
    Guid ActionTakenByUserId,
    string Action,
    string? Notes = null
) : IRequest<bool>;
