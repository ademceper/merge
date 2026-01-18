using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetAgentDashboard;

public record GetAgentDashboardQuery(
    Guid AgentId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<SupportAgentDashboardDto>;
