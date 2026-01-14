using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCampaignAnalyticsQuery(Guid CampaignId) : IRequest<EmailCampaignAnalyticsDto?>;
