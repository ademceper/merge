using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Queries.GetStreamStats;

public record GetStreamStatsQuery(Guid StreamId) : IRequest<LiveStreamStatsDto>;
