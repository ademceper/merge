using MediatR;

namespace Merge.Application.Support.Commands.ReopenTicket;

public record ReopenTicketCommand(
    Guid TicketId
) : IRequest<bool>;
