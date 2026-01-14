using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetUnassignedTickets;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUnassignedTicketsQuery() : IRequest<IEnumerable<SupportTicketDto>>;
