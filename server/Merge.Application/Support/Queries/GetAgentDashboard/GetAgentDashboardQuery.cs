using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetAgentDashboard;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAgentDashboardQuery(
    Guid AgentId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<SupportAgentDashboardDto>;
