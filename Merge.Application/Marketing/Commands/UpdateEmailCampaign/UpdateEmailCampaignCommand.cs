using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.UpdateEmailCampaign;

public record UpdateEmailCampaignCommand(
    Guid Id,
    string? Name,
    string? Subject,
    string? FromName,
    string? FromEmail,
    string? ReplyToEmail,
    Guid? TemplateId,
    string? Content,
    DateTime? ScheduledAt,
    string? TargetSegment) : IRequest<EmailCampaignDto>;
