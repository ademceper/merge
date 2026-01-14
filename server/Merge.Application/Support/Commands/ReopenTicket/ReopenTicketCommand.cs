using MediatR;

namespace Merge.Application.Support.Commands.ReopenTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ReopenTicketCommand(
    Guid TicketId
) : IRequest<bool>;
