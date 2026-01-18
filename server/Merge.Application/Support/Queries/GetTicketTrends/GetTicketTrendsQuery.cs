using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketTrends;

public record GetTicketTrendsQuery(
    Guid? AgentId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<TicketTrendDto>>;
