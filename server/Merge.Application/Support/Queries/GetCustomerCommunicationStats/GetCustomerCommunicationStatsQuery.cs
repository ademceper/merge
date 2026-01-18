using MediatR;

namespace Merge.Application.Support.Queries.GetCustomerCommunicationStats;

public record GetCustomerCommunicationStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Dictionary<string, int>>;
