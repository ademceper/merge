using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetEmailCampaignById;

public record GetEmailCampaignByIdQuery(
    Guid Id) : IRequest<EmailCampaignDto?>;
