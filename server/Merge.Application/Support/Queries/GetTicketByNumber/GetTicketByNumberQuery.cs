using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketByNumber;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTicketByNumberQuery(
    string TicketNumber,
    Guid? UserId = null
) : IRequest<SupportTicketDto?>;
