using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketsByCategory;

public record GetTicketsByCategoryQuery(
    Guid? AgentId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<CategoryTicketCountDto>>;
