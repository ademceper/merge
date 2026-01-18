using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;

namespace Merge.Application.Support.Queries.GetUserTickets;

public record GetUserTicketsQuery(
    Guid UserId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SupportTicketDto>>;
