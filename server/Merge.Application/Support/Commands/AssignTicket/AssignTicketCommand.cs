using MediatR;

namespace Merge.Application.Support.Commands.AssignTicket;

public record AssignTicketCommand(
    Guid TicketId,
    Guid AssignedToId
) : IRequest<bool>;
