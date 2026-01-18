using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetWorstPerformers;

public record GetWorstPerformersQuery(
    int Limit
) : IRequest<List<TopProductDto>>;

