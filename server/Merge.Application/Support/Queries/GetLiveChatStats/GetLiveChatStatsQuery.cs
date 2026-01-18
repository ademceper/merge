using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Support.Queries.GetLiveChatStats;

public record GetLiveChatStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<LiveChatStatsDto>;
