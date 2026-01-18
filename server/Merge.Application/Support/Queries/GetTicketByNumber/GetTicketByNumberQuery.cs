using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketByNumber;

public record GetTicketByNumberQuery(
    string TicketNumber,
    Guid? UserId = null
) : IRequest<SupportTicketDto?>;
