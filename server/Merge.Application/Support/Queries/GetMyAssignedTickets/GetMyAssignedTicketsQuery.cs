using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetMyAssignedTickets;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetMyAssignedTicketsQuery(
    Guid AgentId
) : IRequest<IEnumerable<SupportTicketDto>>;
