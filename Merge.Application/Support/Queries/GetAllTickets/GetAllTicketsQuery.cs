using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;

namespace Merge.Application.Support.Queries.GetAllTickets;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetAllTicketsQuery(
    string? Status = null,
    string? Category = null,
    Guid? AssignedToId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SupportTicketDto>>;
