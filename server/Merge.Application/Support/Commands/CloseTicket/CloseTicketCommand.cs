using MediatR;

namespace Merge.Application.Support.Commands.CloseTicket;

public record CloseTicketCommand(
    Guid TicketId
) : IRequest<bool>;
