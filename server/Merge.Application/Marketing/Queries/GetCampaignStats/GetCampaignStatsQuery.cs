using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetCampaignStats;

public record GetCampaignStatsQuery() : IRequest<EmailCampaignStatsDto>;
