using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicket;

public record GetTicketQuery(
    Guid TicketId,
    Guid? UserId = null
) : IRequest<SupportTicketDto?>;
