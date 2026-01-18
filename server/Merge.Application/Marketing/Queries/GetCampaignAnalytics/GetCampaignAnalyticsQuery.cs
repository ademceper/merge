using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

public record GetCampaignAnalyticsQuery(Guid CampaignId) : IRequest<EmailCampaignAnalyticsDto?>;
