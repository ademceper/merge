using MediatR;

namespace Merge.Application.Support.Commands.AssignTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AssignTicketCommand(
    Guid TicketId,
    Guid AssignedToId
) : IRequest<bool>;
