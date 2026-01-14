using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketTrends;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTicketTrendsQuery(
    Guid? AgentId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<TicketTrendDto>>;
