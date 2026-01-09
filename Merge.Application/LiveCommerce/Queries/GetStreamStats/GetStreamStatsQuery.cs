using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Queries.GetStreamStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetStreamStatsQuery(Guid StreamId) : IRequest<LiveStreamStatsDto>;

