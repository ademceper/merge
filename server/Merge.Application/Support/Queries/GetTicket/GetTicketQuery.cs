using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTicketQuery(
    Guid TicketId,
    Guid? UserId = null
) : IRequest<SupportTicketDto?>;
