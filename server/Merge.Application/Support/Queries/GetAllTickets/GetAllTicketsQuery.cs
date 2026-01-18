using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Queries.GetAllTickets;

public record GetAllTicketsQuery(
    string? Status = null,
    string? Category = null,
    Guid? AssignedToId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SupportTicketDto>>;
