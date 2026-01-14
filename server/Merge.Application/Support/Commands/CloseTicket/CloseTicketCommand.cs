using MediatR;

namespace Merge.Application.Support.Commands.CloseTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CloseTicketCommand(
    Guid TicketId
) : IRequest<bool>;
