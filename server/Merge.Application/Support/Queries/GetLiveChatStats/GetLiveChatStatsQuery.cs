using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Support.Queries.GetLiveChatStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetLiveChatStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<LiveChatStatsDto>;
