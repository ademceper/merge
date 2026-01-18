using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetUnassignedTickets;

public record GetUnassignedTicketsQuery() : IRequest<IEnumerable<SupportTicketDto>>;
