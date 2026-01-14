using MediatR;

namespace Merge.Application.Marketing.Commands.ScheduleEmailCampaign;

public record ScheduleEmailCampaignCommand(
    Guid Id,
    DateTime ScheduledAt) : IRequest<bool>;
