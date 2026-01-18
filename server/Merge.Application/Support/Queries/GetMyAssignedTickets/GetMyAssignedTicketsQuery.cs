using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetMyAssignedTickets;

public record GetMyAssignedTicketsQuery(
    Guid AgentId
) : IRequest<IEnumerable<SupportTicketDto>>;
